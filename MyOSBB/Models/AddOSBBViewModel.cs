﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class AddOSBBViewModel
    {
        public AddTempOSBB addTempOSBB { get; set; }

        public List<OSBB> OSBBList { get; set; }
    }
}