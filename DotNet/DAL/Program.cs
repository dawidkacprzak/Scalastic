using System;

namespace DAL {
    class Program {
        static void Main (string[] args) {
            ElasticController.Instance.StartImportToElastic(
                "test",
                "select t.ID,t.BirthDate,t.Login,t.Password_md5, ISNULL(s.title,'') as title into ElasticCache.dbo.cache from TempUsers t left join dbo.simplejoin s on s.TempUsersID = t.ID where t.BirthDate > '1998-02-13'");
        }
    }
}
