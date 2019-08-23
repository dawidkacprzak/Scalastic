using System;
using System.Collections.Generic;

namespace DAL.Models
{
    public partial class Queries
    {
        public int Id { get; set; }
        public int MinutePeriod { get; set; }
        public byte Active { get; set; }
        public string CountQuery { get; set; }
        public string IndexName { get; set; }
        public string WhereQuery { get; set; }
    }
}
