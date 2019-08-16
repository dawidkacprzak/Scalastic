using Elasticsearch.Net;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using RestSharp;
using Scalastic.Models;
using System.Collections.Generic;

namespace Scalastic
{
    class Program
    {
        static void Main(string[] args)
        {
            Controller.Instance.Init(new Uri("http://localhost:9200"));
            Console.WriteLine(Controller.Instance.IsConnected().ToString());
            var data = Controller.Instance.GetListOfT<CatIndices>("");
            foreach (var item in data)
            {
                Console.WriteLine(item.Index);
            }
            Console.ReadKey();
        }
    }
}
//curl -XGET "http://localhost:9200/_cat/shards?format=json"