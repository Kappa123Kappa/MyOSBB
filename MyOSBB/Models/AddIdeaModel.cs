using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class AddIdeaModel
    {
        public Idea idea { get; set; }

        public List<OSBB> OSBBList { get; set; }
    }
}