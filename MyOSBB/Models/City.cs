using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class City
    {
        public int Id { get; set; }

        public int IdDistrict { get; set; }

        public  string Name { get; set; }
    }
}