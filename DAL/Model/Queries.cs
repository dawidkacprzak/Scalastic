using System;
using System.Collections.Generic;

namespace DAL.Model
{
    public partial class Queries
    {
        public int Id { get; set; }
        public string Query { get; set; }
        public int MinutePeriod { get; set; }
        public byte Active { get; set; }
    }
}
