using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class IdeaModel
    {
        public int id { get; set; }

        public int idOSBB { get; set; }

        public string title { get; set; }

        public string text { get; set; }

        public string pathToPhoto { get; set; }

        public DateTime date { get; set; }

        public string author { get; set; }

        public bool isVouted { get; set; }

        public int quantityOfVotes { get; set; }

        public int quantityOfY { get; set; }

        public int quantityOfN { get; set; }
    }
}