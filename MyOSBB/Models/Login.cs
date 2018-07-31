using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyOSBB.Models
{
    public class Login
    {
        [Required(ErrorMessage = "Невірний логін")]
		//[StringLength(25, MinimumLength = 3, ErrorMessage = "Невірний логін")]
		[Remote("CheckName", "Account", ErrorMessage = "Name is not valid.")]
		public string UserLogin { get; set; }
        [Required(ErrorMessage = "Невірний пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public bool IsRemembered { get; set; }
    }
}