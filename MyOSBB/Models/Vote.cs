using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class Vote
    {
        public int Id { get; set; }

        public int IdOSBB { get; set; }

        public int IdIdea { get; set; }

        public int IdUser { get; set; }

        public int UserVote { get; set; }
    }
}