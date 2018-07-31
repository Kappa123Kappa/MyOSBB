using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class New
    {
        public int Id { get; set; }

        public int IdOSBB { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public string PathToPhoto { get; set; }

        public DateTime Date { get; set; }

        public string Author { get; set; }

    }
}