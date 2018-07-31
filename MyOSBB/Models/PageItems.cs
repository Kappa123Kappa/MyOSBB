using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class PageItems
    {
        public string Active { get; set; }
        public string Action { get; set; }
        public string IClass { get; set; }
        public string MenuItem { get; set; }

        public PageItems(){ }

        public PageItems(string active, string action, string iClass, string menuItem)
        {
            this.Active = active;
            this.Action = action;
            this.IClass = iClass;
            this.MenuItem = menuItem;
        }
    }
}