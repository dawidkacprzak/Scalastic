using Elasticsearch.Net;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using RestSharp;
using System.Collections.Generic;
using DAL.Model;
namespace Scalastic
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> lista = new List<int>(){
                1,2,4,5,6,7,3,4,6,353,25,1,1,32,5,526
            };

            while(true){
                Console.WriteLine("1. Zobacz wszystkie zapytania");
                Console.WriteLine("2. Dodaj ");
                Console.ReadKey();
            }
        }
    }
}