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
            Controller.Init(new Uri("http://localhost:9200"));
            Console.WriteLine(Controller.IsConnected().ToString());
            Console.ReadKey();
        }
    }
}
//curl -XGET "http://localhost:9200/_cat/shards?format=json"