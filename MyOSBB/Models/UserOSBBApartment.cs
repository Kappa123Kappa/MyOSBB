using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class UserOSBBApartment
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public int IdOSBB { get; set; }
        public int ApartmentNumber { get; set; }
    }
}