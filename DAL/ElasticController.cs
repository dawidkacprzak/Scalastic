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

        int fetchedDocs = 0;
        private int maximumInstancesOfThreadsForCompleteQuery = 0;
        int startedThreads = 0;

        public void StartImportToElastic(string indexName,string queryWithWhereID,string countQuery)
        {
            ClearSession();
            if(!eclient.Ping().IsValid)
            {
                Console.WriteLine("Blad podczas pingowania elastic'a - sprawdz polaczenie z serwerem i czy przypadkiem polaczenie nie jest blokowane.");
                return;
            }
            int queryResultCount = GetQueryRowCount(countQuery);
            int threadCount;

            startedThreads = 0;
            if (queryResultCount > 0)
                threadCount = decimal.ToInt32(Math.Ceiling(((decimal)queryResultCount) / paginationSize));
            else return;

            if (threadCount < maxThreadRunCount)
            {
                for (int i = 0; i < threadCount; i++)
                {
                    int c = i;
                    CallbackTask(indexName, c, queryWithWhereID, true, false);
                    Thread.Sleep(2000);
                }
            }
            else
            {
                maximumInstancesOfThreadsForCompleteQuery = threadCount;
                startedThreads = maxThreadRunCount - 1;

                for (int i = 0; i <= maxThreadRunCount; i++)
                {
                    int c = i;
                    Task t = new Task(()=>CallbackTask(indexName, c, queryWithWhereID, true, true));
                    tasks.Add(t);
                }
            }

            foreach (var item in tasks)
                item.Start();

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Zakonczono migracje");
        }

        private void CallbackTask(string indexName, int page, string queryWithWhereID, bool isRootTask, bool forceNewTask)
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
                BulkQuery(indexName, queryWithWhereID, currentValue, forceNewTask);
            }
        }

        private void BulkQuery(string indexName, string query, int pageCount, bool forceNewTask)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                try
                {
                    string searchQuery = PreparePaginationQuery(query.Clone().ToString(), pageCount);
                    Console.WriteLine(searchQuery);
                    SqlCommand command = new SqlCommand(searchQuery, con);
                    SqlDataReader reader = command.ExecuteReader();
                    List<Dictionary<string,object>> list = new List<Dictionary<string,object>>();
                    
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            list.Add(new Dictionary<string,object>(){
                                {reader.GetName(i), reader.GetValue(i)}
                            });
                        }
                    }
       
                    var waitHandle = new CountdownEvent(1);
                    var descriptor = new BulkDescriptor();

                    var bulkAll = eclient.BulkAll(list, b => b
                        .Index(indexName)
                        .BackOffRetries(2)
                        .BackOffTime("30s")
                        .RefreshOnCompleted(true)
                        .MaxDegreeOfParallelism(4)
                        .Size(paginationSize)
                    );

                    bulkAll.Subscribe(new BulkAllObserver(
                        onNext: (b) =>
                        {
                            fetchedDocs += b.Items.Count();
                        },
                        onError: (e) => { Console.WriteLine("bulkFailed"); throw e; },
                        onCompleted: () => { waitHandle.Signal();
                            Console.WriteLine("Łacznie znaleziono: " + fetchedDocs + " elementow");
                        }
                    ));

                    waitHandle.Wait();

                    if (forceNewTask)
                    {
                        con.Close();
                        CallbackTask(indexName, pageCount, query, false, true);
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.Sleep(5000);
                    BulkQuery(indexName, query, pageCount, forceNewTask);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        #region helpermethods

        private string PreparePaginationQuery(string query,int page)
        {
            query = query.Replace("~", ((paginationSize * page)).ToString());
            query = query.Replace("§", ((paginationSize * (page + 1))).ToString());
            return query;
        }

        private void ClearSession(){
            startedThreads = 0;
            tasks.Clear();
            fetchedDocs = 0;
            maximumInstancesOfThreadsForCompleteQuery = 0;
        }

        private int GetQueryRowCount(string countQuery)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand countCommand = new SqlCommand(countQuery, con);
                int count = (int)countCommand.ExecuteScalar();
                con.Close();
                return count;
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