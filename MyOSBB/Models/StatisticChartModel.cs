using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class StatisticChartModel
    {
        public int Month { get; set; }
        public int IdTypeServices { get; set; }
        public decimal AverageSumService { get; set; }
    }
}