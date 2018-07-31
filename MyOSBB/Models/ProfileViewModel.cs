using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class ProfileViewModel
    {
		public ChangeUserServicesNumbersModel changeUserServicesNumbersModel { get; set; }

		public ChangePasswordModel changePasswordModel { get; set; }

        public Registration registration { get; set; }

        public List<OSBB> OSBBList { get; set; }
    }
}