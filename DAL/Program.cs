using System;

namespace DAL {
    class Program {
        static void Main (string[] args) {
            //+ = where id > <
            // - and id <>
            ElasticController.Instance.StartImportToElastic("select * from TempUsers","select * from TempUsers where ID >= ~ and ID < §");
        }
    }
}