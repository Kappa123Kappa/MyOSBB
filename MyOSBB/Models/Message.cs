using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int IdOSBB { get; set; }
        public string User { get; set; }
        public DateTime MessageDate { get; set; }
        public string Text { get; set; }
    }
}