using System;
using System.Collections.Generic;
using System.Text;

namespace Scalastic.Models
{
    public class CatIndices
    {
        public string Health { get; set; }
        public string Status { get; set; }
        public string Index { get; set; }
        public string Uuid { get; set; }
        public long? Pri { get; set; }
        public long? Rep { get; set; }
        public long? DocsCount { get; set; }
        public long? DocsDeleted { get; set; }
        public string StoreSize { get; set; }
        public string PriStoreSize { get; set; }
    }
}
