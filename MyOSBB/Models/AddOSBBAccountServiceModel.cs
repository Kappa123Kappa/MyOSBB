using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
	public class AddOSBBAccountServiceModel
	{
		public int IdOSBB { get; set; }
		public string AccountTypeName { get; set; }
		public string AccountNumber { get; set; }
	}
}