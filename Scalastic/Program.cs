using Elasticsearch.Net;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using RestSharp;
using System.Collections.Generic;
using DAL.Models;
using DAL;

namespace Scalastic
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var ctx = new ElasticContext())
            {
            while (true)
            {
                Console.WriteLine("1. Zobacz wszystkie zapytania");
                Console.WriteLine("2. Dodaj nowe zapytanie");
                Console.WriteLine("3. Wykonaj testowo zapytanie");
                int choice = int.Parse(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        using (var ctx = new ElasticContext())
                        {
                            foreach (var item in ctx.Queries.ToList())
                            {
                                Console.WriteLine(item.Query);
                            }
                        }
                        break;
                    case 2:
                        int minutePeriod;
                        string Query, IndexName;
                        bool Active;
                        Console.WriteLine("Podaj nazwe indeksu pod jakim będzie zapisywane zapytanie\n");
                        IndexName = Console.ReadLine().ToLower();
                        Console.WriteLine("Podaj tresc zapytania ktore zrobi tabele cache - select ID,X,Y into ElasticCache.elastic_cache from X\n");
                        Query = Console.ReadLine();
                        Console.WriteLine("Czy na starcie zapytanie ma byc aktywne? 0/1");
                        int z = int.Parse(Console.ReadLine());
                        if (z == 1) Active = true; else Active = false;
                        Console.WriteLine("Podaj czas w minutach (bez liter) co ile zapytanie ma sie wykonywac");
                        minutePeriod = int.Parse(Console.ReadLine());
                        using (var ctx = new ElasticContext())
                        {
                            ctx.Queries.Add(new Queries()
                            {
                                Active = Active ? (byte)0x1 : (byte)0x0,
                                Query = Query,
                                IndexName = IndexName,
                                MinutePeriod = minutePeriod
                            });
                            ctx.SaveChanges();
                        }
                        break;
                    case 3:
                        using (var ctx = new ElasticContext())
                        {
                            int index = 0;
                            foreach (var item in ctx.Queries.ToList())
                            {
                                Console.WriteLine(index + " -> " + item.IndexName);
                                index++;
                            }
                            int w = int.Parse(Console.ReadLine());
                            var query = ctx.Queries.ToList().ElementAt(w);
                            ElasticController.Instance.StartImportToElastic(query.IndexName, query.Query);
                        }
                        break;
                }
                Console.ReadKey();
                Console.Clear();
            }
        }
    }
}