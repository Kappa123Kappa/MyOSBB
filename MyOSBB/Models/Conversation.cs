using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class Conversation
    {
        public Conversation()
        {
            Status = messageStatus.Sent.ToString();
        }

        public enum messageStatus
        {
            Sent,
            Delivered
        }

        public int Id { get; set; }
        public int IdSender { get; set; }
        public int IdReceiver { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}