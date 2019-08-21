using System;
using DAL.Model;
namespace DAL
{
    class Program
    {
        static void Main(string[] args)
        {
            int? f = 3;
            int g = 3;

            x(f);
            x(g);

            x(null);

        }

        static void x(int a){
            Console.WriteLine("PIERWSZA");
        }

        static void x(int? a){
            Console.WriteLine("DRUGA");

        }
    }
}

