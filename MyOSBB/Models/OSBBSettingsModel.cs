using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class OSBBSettingsModel
    {
		public AddOSBBAccountServiceModel addOSBBAccountServiceModel { get; set; }

		public OSBBAccountService osbbAccountService { get; set; }

		public OSBB selectedOSBB { get; set; }

        public List<OSBB> OSBBList { get; set; }
    }
}