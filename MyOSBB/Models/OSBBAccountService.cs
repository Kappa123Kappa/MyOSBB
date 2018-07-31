using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class OSBBAccountService
    {
        public int Id { get; set; }
        public int IdOSBB { get; set; }
        public int IdType { get; set; }
        public string AccountNumber { get; set; }
    }
}