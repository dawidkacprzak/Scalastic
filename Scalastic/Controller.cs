using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scalastic
{
    public sealed class Controller
    {
        #region singleton
        private static Controller instance = null;
        private static readonly object padlock = new object();
        private Controller() { }
        public static Controller Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new Controller();
                    return instance;
                }
            }
        }
        #endregion
        private Uri elasticHost;
        private RestClient client;
        public void Init(Uri host)
        {
            elasticHost = host;
            client = new RestClient(host);
        }

        public bool IsConnected()
        {
            try
            {
                var response = client.Execute(new RestRequest());
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return false;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public List<T> GetListOfT<T>(string endpoint)
        {
            var response = client.Execute(new RestRequest("/_cat/"+endpoint+"?format=json", Method.GET));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(response.Content);

        }
    }
}
