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

namespace DAL {
    public sealed class ElasticController
    {
        #region singleton
        private static object lockPad = new object();        
        private ElasticClient eclient;
        private string connectionString = "Data Source=51.254.205.149;Initial Catalog=Elastic;User ID=rekurencja;Password=Hermetyzacj4!";
        private static ElasticController instance;
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
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("tempusers");
            eclient = new ElasticClient(settings);
        }
        #endregion
        
        public void StartImportToElastic(string query, string queryWithWhereID)
        {
            string countQuery = "select count(*) " + query.Substring(query.IndexOf("from"));
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand countCommand = new SqlCommand(countQuery, con);
            int count = (int)countCommand.ExecuteScalar();

            int threadCount = 0;
            if (count == 0) return;
            else
            {
                decimal div = (decimal)(count / 700.0);
                threadCount = Decimal.ToInt32(Math.Ceiling(div));
            }

            if (threadCount < 5)
            {
                for (int i = 0; i < threadCount; i++)
                {
                    int c = i;
                    CallbackTask(c, queryWithWhereID,true, false);
                }
            }
            else
            {
                threadLimit = threadCount;
                for (int i = 0; i < 5;i++){
                    startedThreads++;
                }
                for (int i = 0; i < 5; i++)
                {
                    int c = i;
                    CallbackTask(c, queryWithWhereID,true, true);
                }
            }
            con.Close();
        }

        private int threadLimit;
        int startedThreads = 0;

        async void CallbackTask(int page, string queryWithWhereID,bool initCallback, bool forceNewTask)
        {
            int reserved = startedThreads++;
            int currentValue;
            if(initCallback){
                currentValue = page;
            }else { 
                currentValue = reserved; 
                if(currentValue>threadLimit) return;
            }

            await Task.Run(() =>
            {
                Thread myNewThread = new Thread(new ThreadStart(() => GetData(queryWithWhereID, currentValue, forceNewTask)));
                myNewThread.Start(); 
            });
        }
        private void GetData(string query, int pageCount, bool forceNewTask)
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            var newquery = query.Clone().ToString();
            newquery = newquery.Replace("~", ((700 * pageCount)).ToString());
            newquery = newquery.Replace("§", ((700 * (pageCount + 1))).ToString());
            SqlCommand command = new SqlCommand(newquery, con);
            SqlDataReader reader = command.ExecuteReader();
            List<TempUsers> list = new List<TempUsers>();
            while (reader.Read())
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dic.Add(reader.GetName(i), reader.GetValue(i).ToString());
                }
                TempUsers example = DictionaryToObject<TempUsers>(dic);
                list.Add(example);
            }
            BulkDescriptor descriptor = new BulkDescriptor();
            foreach (var item in list)
            {
                descriptor.Index<TempUsers>(d => d.Document(item));
            }
            var x = eclient.Bulk(descriptor);
            Console.WriteLine("done");
            if (forceNewTask)
            {
                CallbackTask(pageCount, query, false,true);
            }
            con.Close();
        }

        private static T DictionaryToObject<T>(IDictionary<string, string> dict) where T : new()
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
    }
}