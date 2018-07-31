using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyOSBB.Models
{
    public class ChangePasswordModel
    {
        public int IdOSBB { get; set; }

        public int IdUser { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }

        public string NewPasswordConfirm { get; set; }
    }
}