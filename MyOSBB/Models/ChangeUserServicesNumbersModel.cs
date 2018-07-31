using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
	public class ChangeUserServicesNumbersModel
	{
		public int IdOSBB { get; set; }

		public int ApartmentNumber { get; set; }

		public string GazNumber { get; set; }

		public string WaterNumber { get; set; }

		public string ElectricityNumber { get; set; }
	}
}