﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
	public class ForgotPasswordViewModel
	{
		[Required]
		public string Email { get; set; }
	}
}