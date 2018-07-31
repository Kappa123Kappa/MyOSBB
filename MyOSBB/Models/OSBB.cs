using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class OSBB
    {
        public int Id { get; set; }

        public int IdRegion { get; set; }

        public int IdDistrict { get; set; }
        
        public int IdCity { get; set; }

        public string Name { get; set; }

        public int IdHead { get; set; }

        public int quantityOfFlats { get; set; }

        public bool isRegistered { get; set; }
    }
}