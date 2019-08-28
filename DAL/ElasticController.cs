using System.Xml.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using DAL.Models;
using System.Data.SqlClient;
using Elasticsearch.Net;
using Nest;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DAL {
    public sealed class ElasticController
    {
        #region singleton
        private static object lockPad = new object();        
        private ElasticClient eclient;
        private const int paginationSize = 10000;
        private const int maxThreadRunCount = 4;
        private string connectionString = "Data Source=51.254.205.149;Initial Catalog=Elastic;User ID=rekurencja;Password=Hermetyzacj4!";
        private static ElasticController instance;
        List<Task> tasks = new List<Task>();
        public static ElasticController Instance
        {
            get
            {
                lock (lockPad)
                {
                    if (instance == null)
                        instance = new ElasticController();
                }
                return instance;
            }
        }
        private ElasticController()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"));
            eclient = new ElasticClient(settings);
        }
        #endregion

        private int maximumInstancesOfThreadsForCompleteQuery = 0;
        int startedThreads = 0;

        public void StartImportToElastic(string indexName,string query)
        {
            ClearSession();
            if(!eclient.Ping().IsValid)
            {
                Console.WriteLine("Blad podczas pingowania elastic'a - sprawdz polaczenie z serwerem i czy przypadkiem polaczenie nie jest blokowane.");
                return;
            }
            int queryResultCount = GetQueryRowCount(query);
            int threadCount;

            startedThreads = 0;
            if (queryResultCount > 0)
                threadCount = decimal.ToInt32(Math.Ceiling(((decimal)queryResultCount) / paginationSize));
            else return;

            if (threadCount < maxThreadRunCount)
            {
                maximumInstancesOfThreadsForCompleteQuery = threadCount;

                for (int i = 0; i < threadCount; i++)
                {
                    int c = i;
                    Task t = new Task(() => CallbackTask(indexName, c, true, false));
                    tasks.Add(t);
                }
            }
            else
            {
                maximumInstancesOfThreadsForCompleteQuery = threadCount;
                startedThreads = maxThreadRunCount - 1;

                for (int i = 0; i <= maxThreadRunCount; i++)
                {
                    int c = i;
                    Task t = new Task(()=>CallbackTask(indexName, c, true, true));
                    tasks.Add(t);
                }
            }

            foreach (var item in tasks)
                item.Start();

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Zakonczono migracje");
            GC.Collect();
        }

        private void CallbackTask(string indexName, int page,bool isRootTask, bool forceNewTask)
        {
            int reserved = 0;
            int currentValue = 0;

            if (!isRootTask)
                reserved = ++startedThreads;
            if (isRootTask)
                currentValue = page;
            else
                currentValue = reserved;

            if (currentValue <= maximumInstancesOfThreadsForCompleteQuery)
            {
                BulkQuery(indexName, currentValue, forceNewTask);
            }
        }

        private void BulkQuery(string indexName, int pageCount, bool forceNewTask)
        {
            BulkDescriptor descriptor = new BulkDescriptor();
            int indexedCount = 0;
            bool successFlag = true;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                try
                {
                    string searchQuery = PreparePaginationQuery(pageCount);
                    Console.WriteLine(searchQuery);
                    using (SqlCommand command = new SqlCommand(searchQuery, con))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Dictionary<string, object> temp = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                        temp.Add(reader.GetName(i), reader.GetValue(i));
                                }
                                indexedCount++;
                                descriptor.Index<Dictionary<string, object>>(op => op
                                .Document(temp)
                                .Index(indexName)
                                .Id(temp["ID"].ToString()));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    successFlag = false;
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(5000);
                    BulkQuery(indexName, pageCount, forceNewTask);
                }
                finally
                {
                    if (successFlag)
                    {
                        if (indexedCount > 0)
                        {
                            var waitHandle = new CountdownEvent(1);
                            var bulkAll = eclient.Bulk(descriptor);
                            waitHandle.Signal();
                            if (bulkAll.IsValid)
                            {
                                Console.WriteLine("Polecenie bulk zakonczone pomyslnie, zaimportowano kolejne " + bulkAll.Items.Count + " wierszy");
                            }
                            else
                            {
                                Console.WriteLine(bulkAll.OriginalException);
                            }
                        }
                        if (forceNewTask)
                        {
                            con.Close();
                            CallbackTask(indexName, pageCount, false, true);
                        }
                    }
                }
            }
        }

        #region helpermethods

        private string PreparePaginationQuery(int page)
        {
            string q = "Select * from ElasticCache.dbo.cache with (nolock) where ID > ~ AND ID <= §;";
            q = q.Replace("~", ((paginationSize * page)).ToString());
            q = q.Replace("§", ((paginationSize * (page + 1))).ToString());
            return q;
        }

        private void ClearSession(){
            startedThreads = 0;
            tasks.Clear();
            maximumInstancesOfThreadsForCompleteQuery = 0;
        }

        private int GetQueryRowCount(string countQuery)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                Console.WriteLine("Uruchamianie raportu i tworzenie widoku..");
                using (SqlCommand drop = new SqlCommand("Drop table if EXISTS ElasticCache.dbo.elastic_cache", con))
                {
                    drop.ExecuteNonQuery();
                    using (SqlCommand countCommand = new SqlCommand(countQuery, con))
                    {
                        countCommand.CommandTimeout = 9999;
                        countCommand.ExecuteNonQuery();
                        Console.WriteLine("Zakonczono. Przystępowanie do importu danych");
                        using (SqlCommand command = new SqlCommand("select MAX(ID) from ElasticCache.dbo.cache with (nolock)", con))
                        {
                            int count = (int)command.ExecuteScalar();
                            return count;
                        }

                    }
                }
            }
        }

        private T DictionaryToObject<T>(IDictionary<string, string> dict) where T : new()
        {
            var t = new T();
            PropertyInfo[] properties = t.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (!dict.Any(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;
                KeyValuePair<string, string> item = dict.First(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));
                Type tPropertyType = t.GetType().GetProperty(property.Name).PropertyType;
                Type newT = Nullable.GetUnderlyingType(tPropertyType) ?? tPropertyType;
                object newA = Convert.ChangeType(item.Value, newT);
                t.GetType().GetProperty(property.Name).SetValue(t, newA, null);
            }
            return t;
        }
        #endregion
    }
}