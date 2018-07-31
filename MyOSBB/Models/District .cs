using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class District
    {
        public int Id { get; set; }

        public int IdRegion { get; set; }

        public string Name { get; set; }
    }
}