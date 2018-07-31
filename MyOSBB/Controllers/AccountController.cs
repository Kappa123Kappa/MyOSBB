using Microsoft.AspNetCore.Identity;
using MyOSBB.DAL;
using MyOSBB.Filter;
using MyOSBB.Models;
using MyOSBB.Providers;
using MyOSBB.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace MyOSBB.Controllers
{
	[AllowAnonymous]
	public class AccountController : Controller
	{
		MyOSBBContext myOSBB = new MyOSBBContext();

		public ActionResult Login()
		{
			if (Request.IsAuthenticated)
			{
				return RedirectToAction("Main", "Main");
			}
			return View();
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult Login(Login model, string returnUrl)
		{
			var checkBox = Request.Form["checkbox1"];

			if (checkBox == "true")
			{
				model.IsRemembered = true;
			}
			else
			{
				model.IsRemembered = false;
			}
			if (ModelState.IsValid)
			{
				if (Membership.ValidateUser(model.UserLogin, model.Password))
				{
					FormsAuthentication.SetAuthCookie(model.UserLogin, model.IsRemembered);
					if (Url.IsLocalUrl(returnUrl))
					{
						return Redirect(returnUrl);
					}
					else
					{
						return RedirectToAction("Main", "Main");
					}
				}
				else
				{
					ModelState.AddModelError("", "Логін або пароль невірні");
					ViewBag.Error = "Логін або пароль невірні";
					return View();
					//return RedirectToAction("Login", "Account");
				}
			}

			return View(model);
		}

		[AllowAnonymous]
		public ActionResult ForgotPassword()
		{
			if (Request.IsAuthenticated)
			{
				return RedirectToAction("Main", "Main");
			}
			return View();
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = myOSBB.Users.FirstOrDefault(u => u.Email == model.Email);
				if (user == null)
				{
					ViewBag.EmailError = "Користувача з такою поштовою скринькою не існує";
					return View("ForgotPassword");
				}
				Random R = new Random();
				user.CodeForResetPassword = Crypto.HashPassword(((long)R.Next(0, 100000) * (long)R.Next(0, 100000)).ToString().PadLeft(10, '0'));
				CustomMembershipProvider customMembershipProvider = new CustomMembershipProvider();
				customMembershipProvider.UpdateUser(user.Id, user.Login, user.Password,
					user.FirstName, user.LastName, user.MiddleName, user.Email, user.Phone, user.CodeForResetPassword);
				EmailService emailService = new EmailService();
				var callbackUrl = Url.Action("ResetPassword", "Account",
					new { userId = user.Id, password = user.CodeForResetPassword }, protocol: Request.Url.Scheme);
				emailService.SendEmail(user.Email, "Зміна паролю", "Для зміни пароля перейдіть по посиланню " + callbackUrl + "\"");

				return RedirectToAction("ForgotPasswordConfirmation", "Account");
			}
			return View(model);
		}

		[AllowAnonymous]
		public ActionResult ForgotPasswordConfirmation()
		{
			if (Request.IsAuthenticated)
			{
				return RedirectToAction("Main", "Main");
			}
			return View();
		}
		
		[AllowAnonymous]
		public ActionResult ResetPassword(string userId, string password)
		{
			if (Request.IsAuthenticated)
			{
				return RedirectToAction("Main", "Main");
			}
			if(userId != null && password != null)
			{
				int idUser;
				if (Int32.TryParse(userId, out idUser))
				{
					User user = myOSBB.Users.FirstOrDefault(u => u.Id == idUser);
					if(user.CodeForResetPassword == password)
					{
						ViewBag.UserId = user.Id;
						user.CodeForResetPassword = null;
						CustomMembershipProvider customMembershipProvider = new CustomMembershipProvider();
						customMembershipProvider.UpdateUser(user.Id, user.Login, user.Password,
							user.FirstName, user.LastName, user.MiddleName, user.Email, user.Phone, user.CodeForResetPassword);
						return View();
					}
					else
					{
						return RedirectToAction("Main", "Main");
					}
					
				}
				else
				{
					return RedirectToAction("Main", "Main");
				}
			}

			return RedirectToAction("Main", "Main");
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult ResetPassword(ResetPassword model)
		{
			if (ModelState.IsValid)
			{
				User user = myOSBB.Users.FirstOrDefault(u => u.Id == model.UserId);
				if (user == null)
				{
					ViewBag.EmailError = "Користувача з такою поштовою скринькою не існує";
					return View("ForgotPassword");
				}
				if(model.Password == model.PasswordConfirm)
				{
					user.Password = Crypto.HashPassword(model.Password);
				}
				CustomMembershipProvider customMembershipProvider = new CustomMembershipProvider();
				customMembershipProvider.UpdateUser(user.Id, user.Login, user.Password,
					user.FirstName, user.LastName, user.MiddleName, user.Email, user.Phone, user.CodeForResetPassword);

				return RedirectToAction("Login", "Account");
			}
			return View(model);
		}

		public ActionResult Registration()
		{
			if (Request.IsAuthenticated)
			{
				return RedirectToAction("Main", "Main");
			}
			return View();
		}

		[HttpPost]
		public ActionResult Registration(Registration model)
		{
			if (myOSBB.Users.FirstOrDefault(u => u.Login == model.Login) != null)
			{
				ViewBag.LoginError = "Користувач з таким логіном вже існує";
			}
			if (myOSBB.Users.FirstOrDefault(u => u.Email == model.Email) != null)
			{
				ViewBag.EmailError = "Користувач з такою поштовою скринькою вже існує"; ;
			}

			if (ModelState.IsValid)
			{
				MembershipUser membershipUser = ((CustomMembershipProvider)Membership.Provider).CreateUser(model.Login, model.Password, model.FirstName,
						model.LastName, model.MiddleName, model.Email, model.Phone);

				if (membershipUser != null)
				{
					FormsAuthentication.SetAuthCookie(model.Login, false);
					return RedirectToAction("Main", "Main");
				}
				else
				{
					ModelState.AddModelError("", "Не вдалось зареєструватись!");
					return View();
				}
			}
			return View();
		}

		[HttpGet]
		public JsonResult CheckName(string name)
		{
			var result = !(myOSBB.Users.FirstOrDefault(u => u.Login == name).Login == null);
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Account()
		{

			return View();
		}
	}
}