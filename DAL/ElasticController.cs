using System;
using Elasticsearch.Net;

namespace DAL
{
    public sealed class ElasticController
    {
        #region singleton
        private static object lockPad = new object();
        private static ElasticController instance;
        public static ElasticController Instance {
            get{
                lock(lockPad){
                    if(instance==null)
                        instance = new ElasticController();
                }
                return instance;
            }
        }
        private ElasticController() {
            var settings = new ConnectionConfiguration(new Uri("http://10.10.1.214:9200"))
                .RequestTimeout(TimeSpan.FromMinutes(2));

            var lowlevelClient = new ElasticLowLevelClient(settings);
        }
        #endregion

        
    }
}