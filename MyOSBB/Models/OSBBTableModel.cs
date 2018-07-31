using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class OSBBTableModel
    {
        public string regionName { get; set; }
        public string districtName { get; set; }
        public string cityName { get; set; }
        public int osbbID { get; set; }
        public string osbbName { get; set; }
        public int quantityOfFlats { get; set; }
        public List<int> flatsList { get; set; }
        public int ApartmentNumber { get; set; }
    }
}