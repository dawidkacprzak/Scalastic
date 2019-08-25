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
                                Console.WriteLine(item.WhereQuery);
                            }
                        }
                        break;
                    case 2:
                        int minutePeriod;
                        string CountQuery, IndexName, WhereQuery;
                        bool Active;
                        Console.WriteLine("Podaj nazwe indeksu pod jakim będzie zapisywane zapytanie\n");
                        IndexName = Console.ReadLine().ToLower();
                        Console.WriteLine("Podaj tresc zapytania ktore zwroci maksymalny indeks zapytania - eg. select max(ID) from X\n");
                        CountQuery = Console.ReadLine();
                        Console.WriteLine("Podaj tresc zapytania wraz z klauzulą where dla ID - eg. select ... from X where ID >= ~ and ID <= §\n");
                        WhereQuery = Console.ReadLine();
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
                                CountQuery = CountQuery,
                                WhereQuery = WhereQuery,
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
                                Console.WriteLine(index + " -> " + item.WhereQuery);
                                index++;
                            }
                            int w = int.Parse(Console.ReadLine());
                            var query = ctx.Queries.ToList().ElementAt(w);
                            ElasticController.Instance.StartImportToElastic(query.IndexName, query.WhereQuery, query.CountQuery);
                        }
                        break;
                }
                Console.ReadKey();
                Console.Clear();
            }
        }
    }
}