using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
	public class ResetPassword
	{
		[Required]
		public int UserId { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required]
		[Compare("Password", ErrorMessage = "Ви ввели різні паролі")]
		[DataType(DataType.Password)]
		public string PasswordConfirm { get; set; }
	}
}