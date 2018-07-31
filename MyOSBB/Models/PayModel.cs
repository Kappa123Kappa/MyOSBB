using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
	public class PayModel
	{
		public int IdUser { get; set; }

		public int IdOSBB { get; set; }

		public int ApartmentNumber { get; set; }

		public string TypeServicesName { get; set; }

		public string AccountNumber { get; set; }

		public string PayButton { get; set; }
	}
}