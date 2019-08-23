using System;

namespace DAL {
    class Program {
        static void Main (string[] args) {
            ElasticController.Instance.StartImportToElastic("produkty", "select ID,CmnKlientID,MrkProduktID,DataWprowadzenia from MrkZamowienie with(nolock) where ID >= ~ and ID <= §", "select MAX(ID) from MrkZamowienie with(nolock)");
        }
    }
}