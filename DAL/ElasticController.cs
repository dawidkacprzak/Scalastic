using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using DAL.Models;
using Elasticsearch.Net;
using Nest;

namespace DAL {
    public sealed class ElasticController {
        #region singleton
        private static object lockPad = new object ();
        private static ElasticController instance;
        public static ElasticController Instance {
            get {
                lock (lockPad) {
                    if (instance == null)
                        instance = new ElasticController ();
                }
                return instance;
            }
        }
        private ElasticController () {

            var settings = new ConnectionSettings (new Uri ("http://localhost:9200")).DefaultIndex ("tempusers");

            var lowlevelClient = new ElasticClient (settings);
            var person = new TempUsers {
                Id = 1,
                Name = "Dawid",
                BirthDate = DateTime.Now,
                Login = "ggg",
                PasswordMd5 = "dsd3f2g2ggwg"

            };

            var ndexResponse = lowlevelClient.IndexDocument (person);
            Console.WriteLine (ndexResponse.OriginalException);
            /* settings.DefaultIndex ("tempusers");

             Console.WriteLine (lowlevelClient.Indices);
             var descriptor = new BulkDescriptor ();
             using (var ctx = new ElasticContext ()) {

                 var documents = ctx.TempUsers.ToList ();
                 foreach (var item in documents) {
                     descriptor.Index<TempUsers> (d => d.Document (item));
                 }
                 var x = lowlevelClient.Bulk (descriptor);
                 Console.WriteLine (x.Errors.ToString ());
             }*/
        }
        #endregion
    }
}