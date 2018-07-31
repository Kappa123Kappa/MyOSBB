using MyOSBB.DAL;
using MyOSBB.Filter;
using MyOSBB.Models;
using MyOSBB.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MyOSBB.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        MyOSBBContext myOSBB = new MyOSBBContext();

        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
                return View(user);
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
	}
}