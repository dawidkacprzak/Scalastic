using System;
using System.Collections.Generic;
using Nest;
namespace DAL.Models {
    public partial class TempUsers {
        public int ID { get; set; }
        public string Login { get; set; }
        public string PasswordMd5 { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Name { get; set; }
    }
}