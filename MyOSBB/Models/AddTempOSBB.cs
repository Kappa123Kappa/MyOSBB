using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class AddTempOSBB
    {
        public int Id { get; set; }

        public int IdUser { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public int IdOSBB { get; set; }

        public string OSBBName { get; set; }

        public int ApartmentNumber { get; set; }

        public string Action { get; set; }  
    }

}