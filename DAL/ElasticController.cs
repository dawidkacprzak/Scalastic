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
        private ElasticController() { }
        #endregion
        
    }
}