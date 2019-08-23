using System;

namespace DAL {
    class Program {
        static void Main (string[] args) {
            ElasticController.Instance.StartImportToElastic("usersi","select * from TempUsers","select * from TempUsers where ID >= ~ and ID < §","select count(ID) from TempUsers");
        }
    }
}