using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class PaidCommunalService
    {
        public int Id { get; set; }

        public int IdUser { get; set; }

        public int IdOSBB { get; set; }

        public int ApartmentNumber { get; set; }

        public int IdTypeServices { get; set; }

        public decimal Sum { get; set; }

        public DateTime Date { get; set; }
    }
}