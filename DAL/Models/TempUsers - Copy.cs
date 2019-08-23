using System;
using System.Collections.Generic;
using Nest;
namespace DAL.Models {
    public partial class MrkZamowienie {
        public int ID { get; set; }
        public int? CmnKlientID { get; set; }
        public int? MrkProduktID { get; set; }
        public DateTime? DataWprowadzenia { get; set; }
    }
}
