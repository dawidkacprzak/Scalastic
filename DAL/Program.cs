using System;

namespace DAL {
    class Program {
        static void Main (string[] args) {
            ElasticController.Instance.StartImportToElastic("tempusers", "select ID,Login,Password_md5,BirthDate,Name from TempUsers where ID >= ~ and ID <= §", "select MAX(ID) from TempUsers");
        }
    }
}
