using System;
using System.Collections.Generic;
using System.Text;

namespace Scalastic.Models
{
    public partial class CatShards
    {
        public string Index { get; set; }
        public long? Shard { get; set; }
        public string Prirep { get; set; }
        public string State { get; set; }
        public long? Docs { get; set; }
        public string Store { get; set; }
        public string Ip { get; set; }
        public string Node { get; set; }
    }
}
