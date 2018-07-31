using MyOSBB.DAL;
using MyOSBB.Filter;
using MyOSBB.Models;
using MyOSBB.Providers;
using MyOSBB.Services;
using PusherServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace MyOSBB.Controllers
{
	public class MainController : Controller
	{
		MyOSBBContext myOSBB = new MyOSBBContext();

		ProfileViewModel profileViewModel = new ProfileViewModel();

		AddOSBBViewModel addOSBBViewModel = new AddOSBBViewModel();

		List<OSBB> osbbVal = new List<OSBB>();

		OSBB selectedOSBB;

		private Pusher pusher;

		List<PageItems> pageItemsList = new List<PageItems>
			{
					new PageItems{Active = "", Action = "Main", IClass = "ti-agenda", MenuItem = "Головна"},
					new PageItems{Active = "", Action = "UserProfile", IClass = "ti-user", MenuItem = "Профіль"},
					new PageItems{Active = "", Action = "MyOSBB", IClass = "ti-home", MenuItem = "Моє ОСББ"},
					new PageItems{Active = "", Action = "MyOSBBSettings", IClass = "ti-settings", MenuItem = "Налаштування ОСББ"},
					new PageItems{Active = "", Action = "Chat", IClass = "ti-comments", MenuItem = "Чат"},
					new PageItems{Active = "", Action = "News", IClass = "ti-bell", MenuItem = "Новини"},
					new PageItems{Active = "", Action = "Ideas", IClass = "fa fa-child" /*ti-alert*/, MenuItem = "Голосування"},
					new PageItems{Active = "", Action = "Statistic", IClass = "ti-stats-up", MenuItem = "Статистика"},
			 };

		private enum OSBBActions
		{
			add, delete
		}

		public MainController()
		{
			var options = new PusherOptions();
			options.Cluster = "eu";

			pusher = new Pusher(
			   "522229",
			   "e9bcc3a4af8977d151f5",
			   "aa53148f11e931b0436b",
			   options
		   );
		}

		[HttpGet]
		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult Main(string id)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;
			ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);
			//List<UserOSBBApartment> userOSBBs = (from UserOSBB in myOSBB.UserOSBBApartments
			//                                     where UserOSBB.IdUser == user.Id
			//                           select UserOSBB).ToList();

			//foreach (var userOSBB in userOSBBs)
			//{
			//    osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == userOSBB.IdOSBB));
			//}

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}



			CreateMenu(0);
			ViewBag.PageItemsList = pageItemsList;


			if (id == null || Int32.Parse(id) == 0)
			{
				if (osbbVal.Count() == 0)
				{
					ViewBag.SelectedOSBB = new OSBB();
					return View(osbbVal);
				}
				if (selectedOSBB != null)
				{
					ViewBag.SelectedOSBB = selectedOSBB;
					return View(osbbVal);
				}
				else
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					selectedOSBB = osbbVal[0];
					ViewBag.News = (from News in myOSBB.News
									where News.IdOSBB == selectedOSBB.Id
									orderby News.Id descending
									select News).Take(6).ToList();

					if (selectedOSBB.quantityOfFlats == myOSBB.UserOSBBApartments.Count(s => s.IdOSBB == selectedOSBB.Id))
					{
						ViewBag.IsAllRegistred = true;
					}
					else
					{
						ViewBag.IsAllRegistred = false;
					}

					var ideas = (from Ideas in myOSBB.Ideas
								 where Ideas.IdOSBB == selectedOSBB.Id
								 orderby Ideas.Date descending
								 select new IdeaModel
								 {
									 id = Ideas.Id,
									 idOSBB = Ideas.IdOSBB,
									 title = Ideas.Title,
									 text = Ideas.Text,
									 pathToPhoto = Ideas.PathToPhoto,
									 date = Ideas.Date,
									 author = Ideas.Author,
									 isVouted = false,
									 quantityOfVotes = 0,
									 quantityOfY = 0,
									 quantityOfN = 0
								 }
					).Take(6).ToList();
					var allUserVotes = (from Votes in myOSBB.Votes
										where Votes.IdOSBB == selectedOSBB.Id
										select Votes).ToList();
					ViewBag.Voters = (from UserOSBBApartment in myOSBB.UserOSBBApartments
									  where UserOSBBApartment.IdOSBB == selectedOSBB.Id
									  select UserOSBBApartment.IdUser).Distinct().Count();
					foreach (var idea in ideas)
					{
						foreach (var vote in allUserVotes)
						{
							if (idea.id == vote.IdIdea && vote.IdUser == user.Id)
							{
								if (vote.UserVote == 1)
								{
									idea.isVouted = true;
									idea.quantityOfVotes++;
									idea.quantityOfY++;
								}
								else if (vote.UserVote == 0)
								{
									idea.isVouted = true;
									idea.quantityOfVotes++;
									idea.quantityOfN++;
								}
							}
							if (idea.id == vote.IdIdea && vote.IdUser != user.Id)
							{
								if (vote.UserVote == 1)
								{
									idea.quantityOfVotes++;
									idea.quantityOfY++;
								}
								else if (vote.UserVote == 0)
								{
									idea.quantityOfVotes++;
									idea.quantityOfN++;
								}
							}
						}
					}
					ViewBag.UserVotes = (from Votes in myOSBB.Votes
										 where Votes.IdOSBB == selectedOSBB.Id
										 select Votes).ToList();
					ViewBag.Ideas = ideas;
					return View(osbbVal);
				}
			}
			else
			{
				int idosbb = Int32.Parse(id);
				//ViewBag.SelectedOSBB = osbbVal.Select(o => o.Name = Request.Form["osbbList"].ToString());
				ViewBag.SelectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idosbb);
				if (ViewBag.SelectedOSBB == null)
				{
					ViewBag.SelectedOSBB = osbbVal[0];
				}
				selectedOSBB = ViewBag.SelectedOSBB;

				ViewBag.News = (from News in myOSBB.News
								where News.IdOSBB == selectedOSBB.Id
								orderby News.Id descending
								select News).Take(6).ToList();

				if (selectedOSBB.quantityOfFlats == myOSBB.UserOSBBApartments.Count(s => s.IdOSBB == selectedOSBB.Id))
				{
					ViewBag.IsAllRegistred = true;
				}
				else
				{
					ViewBag.IsAllRegistred = false;
				}

				var ideas = (from Ideas in myOSBB.Ideas
							 where Ideas.IdOSBB == selectedOSBB.Id
							 orderby Ideas.Date descending
							 select new IdeaModel
							 {
								 id = Ideas.Id,
								 idOSBB = Ideas.IdOSBB,
								 title = Ideas.Title,
								 text = Ideas.Text,
								 pathToPhoto = Ideas.PathToPhoto,
								 date = Ideas.Date,
								 author = Ideas.Author,
								 isVouted = false,
								 quantityOfVotes = 0,
								 quantityOfY = 0,
								 quantityOfN = 0
							 }
					).Take(6).ToList();
				var allUserVotes = (from Votes in myOSBB.Votes
									where Votes.IdOSBB == selectedOSBB.Id
									select Votes).ToList();
				ViewBag.Voters = (from UserOSBBApartment in myOSBB.UserOSBBApartments
								  where UserOSBBApartment.IdOSBB == selectedOSBB.Id
								  select UserOSBBApartment.IdUser).Distinct().Count();
				foreach (var idea in ideas)
				{
					foreach (var vote in allUserVotes)
					{
						if (idea.id == vote.IdIdea && vote.IdUser == user.Id)
						{
							if (vote.UserVote == 1)
							{
								idea.isVouted = true;
								idea.quantityOfVotes++;
								idea.quantityOfY++;
							}
							else if (vote.UserVote == 0)
							{
								idea.isVouted = true;
								idea.quantityOfVotes++;
								idea.quantityOfN++;
							}
						}
						if (idea.id == vote.IdIdea && vote.IdUser != user.Id)
						{
							if (vote.UserVote == 1)
							{
								idea.quantityOfVotes++;
								idea.quantityOfY++;
							}
							else if (vote.UserVote == 0)
							{
								idea.quantityOfVotes++;
								idea.quantityOfN++;
							}
						}
					}
				}
				ViewBag.UserVotes = (from Votes in myOSBB.Votes
									 where Votes.IdOSBB == selectedOSBB.Id
									 select Votes).ToList();
				ViewBag.Ideas = ideas;
				return View(osbbVal);
			}



			//ViewBag.SelectedOSBB = null;
			//return View(osbbVal);
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult UserProfile(string id, string error)
		{

			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;

			// Get user information
			ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);
			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();
			profileViewModel.OSBBList = new List<OSBB>();
			foreach (var userOSBB in userOSBBs)
			{
				profileViewModel.OSBBList.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == userOSBB.osbbID));
			}
			//ViewBag.OSBB = osbb;

			CreateMenu(1);
			ViewBag.PageItemsList = pageItemsList;

            ViewBag.ChangePasswordError = error;


            // Get users apartments
            var osbbTableModel = (from UserOSBBApartment in myOSBB.UserOSBBApartments
								  from OSBB in myOSBB.OSBBs
								  from City in myOSBB.Cities
								  from District in myOSBB.Districts
								  from Region in myOSBB.Regions
								  where ((UserOSBBApartment.IdUser == user.Id) &&
										 (OSBB.Id == UserOSBBApartment.IdOSBB) &&
										 (City.Id == OSBB.IdCity) &&
										 (District.Id == OSBB.IdDistrict) &&
										 (Region.Id == OSBB.IdRegion))
								  select new OSBBTableModel
								  {
									  regionName = Region.Name,
									  districtName = District.Name,
									  cityName = City.Name,
									  osbbID = UserOSBBApartment.IdOSBB,
									  osbbName = OSBB.Name,
									  ApartmentNumber = UserOSBBApartment.ApartmentNumber
								  }
			).ToList<OSBBTableModel>();
			ViewBag.UserOSBBApartments = osbbTableModel;


			// Get users apartments which was not added.
			ViewBag.UserAddTempOSBB = (from UserOSBBApartment in myOSBB.UserOSBBApartments
									   from AddTempOSBBs in myOSBB.AddTempOSBBs
									   where AddTempOSBBs.IdUser == user.Id
									   select AddTempOSBBs).Distinct().ToList();


			ViewBag.UserAccountServices = (from UserAccountServices in myOSBB.UserAccountServices
										   where UserAccountServices.IdUser == user.Id
										   select UserAccountServices).Distinct().ToList();

			if (id == null || Int32.Parse(id) == 0)
			{
				if (profileViewModel.OSBBList.Count() == 0)
				{
					ViewBag.SelectedOSBB = new OSBB();

                    return View(profileViewModel);
				}
				if (selectedOSBB != null)
				{
					ViewBag.SelectedOSBB = selectedOSBB;
                    return View(profileViewModel);
				}
				else
				{
					ViewBag.SelectedOSBB = profileViewModel.OSBBList[0];
					selectedOSBB = profileViewModel.OSBBList[0];
                    return View(profileViewModel);
				}
			}
			else
			{
				int idosbb = Int32.Parse(id);
				//ViewBag.SelectedOSBB = osbbVal.Select(o => o.Name = Request.Form["osbbList"].ToString());
				ViewBag.SelectedOSBB = profileViewModel.OSBBList.FirstOrDefault(o => o.Id == idosbb);
				if (ViewBag.SelectedOSBB == null)
				{
					ViewBag.SelectedOSBB = profileViewModel.OSBBList[0];
				}
                
                selectedOSBB = ViewBag.SelectedOSBB;
				return View(profileViewModel);
			}

			//if (Request.Form["osbbList"] == null)
			//{
			//    if (profileViewModel.OSBBList.Count() == 0)
			//    {
			//        return View(profileViewModel);
			//    }
			//    else if (profileViewModel.OSBBList != null)
			//    {
			//        ViewBag.SelectedOSBB = profileViewModel.OSBBList[0];
			//        // ТУТ
			//        return View(profileViewModel);
			//    }
			//}
			//else
			//{
			//    ViewBag.SelectedOSBB = osbbVal.Select(o => o.Name = Request.Form["osbbList"].ToString());
			//}

			//ViewBag.SelectedOSBB = null;
			//return View(profileViewModel);
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult MyOSBB(string id)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;

			/*
             * 
             * 
             * ТУТ треба буде мінятти
             * 
             * 
             */


			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			string query = "SELECT * FROM UserOSBBApartments WHERE IdUser = @IdUser";

			var data = myOSBB.UserOSBBApartments.SqlQuery(query, new SqlParameter("@IdUser", user.Id)).ToList();

			var osbbTableModel = (from UserOSBBApartment in myOSBB.UserOSBBApartments
								  from OSBB in myOSBB.OSBBs
								  from City in myOSBB.Cities
								  from District in myOSBB.Districts
								  from Region in myOSBB.Regions
								  where ((UserOSBBApartment.IdUser == user.Id) &&
										 (OSBB.Id == UserOSBBApartment.IdOSBB) &&
										 (City.Id == OSBB.IdCity) &&
										 (District.Id == OSBB.IdDistrict) &&
										 (Region.Id == OSBB.IdRegion))
								  select new OSBBTableModel
								  {
									  regionName = Region.Name,
									  districtName = District.Name,
									  cityName = City.Name,
									  osbbID = UserOSBBApartment.IdOSBB,
									  osbbName = OSBB.Name,
									  ApartmentNumber = UserOSBBApartment.ApartmentNumber
								  }
			)/*.Distinct()*/.ToList<OSBBTableModel>();


			//for (int i = 0; i < data.Count(); i++)
			//{
			//    for (int j = 0; j < osbbTableModel.Count(); j++)
			//    {

			//        if (data[i].IdOSBB == osbbTableModel[j].osbbID && osbbTableModel[j].flatsList == null)
			//        {
			//            osbbTableModel[j].flatsList = new List<int>();
			//            osbbTableModel[j].flatsList.Add(data[i].ApartmentNumber);
			//            osbbTableModel[j].quantityOfFlats++;
			//            //continue;
			//        }
			//        else if (data[i].IdOSBB == osbbTableModel[j].osbbID && osbbTableModel[j].flatsList != null)
			//        {
			//            osbbTableModel[j].flatsList.Add(data[i].ApartmentNumber);
			//            osbbTableModel[j].quantityOfFlats++;
			//        }
			//    }
			//}
			ViewBag.UserOSBBApartments = osbbTableModel;
			ViewBag.UserOSBBList = (from UserOSBBApartment in myOSBB.UserOSBBApartments
									from OSBB in myOSBB.OSBBs
									from City in myOSBB.Cities
									from District in myOSBB.Districts
									from Region in myOSBB.Regions
									where ((UserOSBBApartment.IdUser == user.Id) &&
										   (OSBB.Id == UserOSBBApartment.IdOSBB) &&
										   (City.Id == OSBB.IdCity) &&
										   (District.Id == OSBB.IdDistrict) &&
										   (Region.Id == OSBB.IdRegion))
									select new OSBBTableModel
									{
										regionName = Region.Name,
										districtName = District.Name,
										cityName = City.Name,
										osbbID = UserOSBBApartment.IdOSBB,
										osbbName = OSBB.Name,
									}
			).Distinct().ToList<OSBBTableModel>();


			CreateMenu(2);
			ViewBag.PageItemsList = pageItemsList;

			ViewBag.UserAddTempOSBB = (from UserOSBBApartment in myOSBB.UserOSBBApartments
									   from AddTempOSBBs in myOSBB.AddTempOSBBs
									   where AddTempOSBBs.IdUser == user.Id
									   select AddTempOSBBs).Distinct().ToList();

			if (id == null || Int32.Parse(id) == 0)
			{
				if (osbbVal.Count() == 0)
				{
					ViewBag.SelectedOSBB = new OSBB();
					return View(osbbVal);
				}
				if (selectedOSBB != null)
				{

					ViewBag.SelectedOSBB = selectedOSBB;
					return View(osbbVal);
				}
				else
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					selectedOSBB = osbbVal[0];
					if (user.RoleId == 2 && user.Id == ViewBag.SelectedOSBB.IdHead)
					{
						ViewBag.AddTempOSBB = (from UserOSBBApartment in myOSBB.UserOSBBApartments
											   from AddTempOSBBs in myOSBB.AddTempOSBBs
											   where (/*(UserOSBBApartment.IdOSBB == selectedOSBB.Id) &&*/
													  (AddTempOSBBs.IdOSBB == selectedOSBB.Id))
											   select AddTempOSBBs).Distinct().ToList();
					}

					return View(osbbVal);
				}
			}
			else
			{
				int idosbb = Int32.Parse(id);
				//ViewBag.SelectedOSBB = osbbVal.Select(o => o.Name = Request.Form["osbbList"].ToString());
				ViewBag.SelectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idosbb);
				if (ViewBag.SelectedOSBB == null && user.RoleId == 2 && user.Id == osbbVal[0].IdHead)
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					idosbb = osbbVal[0].Id;
					ViewBag.AddTempOSBB = (from UserOSBBApartment in myOSBB.UserOSBBApartments
										   from AddTempOSBBs in myOSBB.AddTempOSBBs
										   where (/*(UserOSBBApartment.IdUser == user.Id) &&*/
												  (AddTempOSBBs.IdOSBB == idosbb))
										   select AddTempOSBBs).Distinct().ToList();
				}
				else if (ViewBag.SelectedOSBB != null && user.RoleId == 2 && user.Id == ViewBag.SelectedOSBB.IdHead)
				{
					ViewBag.AddTempOSBB = (from UserOSBBApartment in myOSBB.UserOSBBApartments
										   from AddTempOSBBs in myOSBB.AddTempOSBBs
										   where (/*(UserOSBBApartment.IdUser == user.Id) &&*/
												  (AddTempOSBBs.IdOSBB == idosbb))
										   select AddTempOSBBs).Distinct().ToList();
				}
				else
				{
					ViewBag.AddTempOSBB = (from UserOSBBApartment in myOSBB.UserOSBBApartments
										   from AddTempOSBBs in myOSBB.AddTempOSBBs
										   where AddTempOSBBs.IdUser == user.Id && AddTempOSBBs.IdOSBB == idosbb
										   select AddTempOSBBs).Distinct().ToList();
				}
				selectedOSBB = ViewBag.SelectedOSBB;
				return View(osbbVal);
			}


			//if (Request.Form["osbbList"] == null)
			//{
			//    if (osbbVal.Count() == 0)
			//    {
			//        return View(osbbVal);
			//    }
			//    else if (osbb != null)
			//    {
			//        ViewBag.SelectedOSBB = osbbVal[0];
			//        // ТУТ
			//        return View(osbbVal);
			//    }
			//}
			//else
			//{
			//    ViewBag.SelectedOSBB = osbbVal.Select(o => o.Name = Request.Form["osbbList"].ToString());
			//}



			//ViewBag.SelectedOSBB = null;
			//return View();
			//ViewBag.PageItemsList = pageItemsList;
			//if (osbb != null)
			//{
			//    return View(osbb);
			//}
			//return View();
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult MyOSBBAdd(string id)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;

			CreateMenu(2);

			ViewBag.PageItemsList = pageItemsList;

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			addOSBBViewModel.OSBBList = new List<OSBB>();

			foreach (var userOSBB in userOSBBs)
			{
				addOSBBViewModel.OSBBList.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == userOSBB.osbbID));
			}

			ViewBag.Regions = new SelectList(myOSBB.Regions, "Id", "Name");

			ViewBag.Districts = new SelectList((from Districts in myOSBB.Districts
												where Districts.IdRegion == 1
												select Districts).ToList(), "Id", "Name");
			ViewBag.Cities = new SelectList(myOSBB.Cities, "Id", "Name");

			if (Request.Form["osbbList"] == null)
			{
				if (addOSBBViewModel.OSBBList.Count() == 0)
				{
                    ViewBag.SelectedOSBB = new OSBB();
                    return View(addOSBBViewModel);
				}
				if (addOSBBViewModel.OSBBList != null)
				{
					ViewBag.SelectedOSBB = addOSBBViewModel.OSBBList[0];
					return View(addOSBBViewModel);
				}
			}
			else
			{
				ViewBag.SelectedOSBB = addOSBBViewModel.OSBBList.Select(o => o.Name = Request.Form["osbbList"].ToString());
			}

			ViewBag.SelectedOSBB = null;


			return View(addOSBBViewModel);
		}


        // Заявка на додавання
        [MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
        public ActionResult AddToTempApartmentForAdding(AddTempOSBB addTempOSBB)
        {
            User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            //addTempOSBB.Id = myOSBB.addTempOSBBs.Count() + 1;
            UserOSBBApartment userOSBBApartments = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.ApartmentNumber == addTempOSBB.ApartmentNumber && u.IdOSBB == addTempOSBB.IdOSBB);
            if (userOSBBApartments == null)
            {
                addTempOSBB.IdUser = user.Id;
                addTempOSBB.FirstName = user.FirstName;
                addTempOSBB.MiddleName = user.MiddleName;
                addTempOSBB.LastName = user.LastName;
                addTempOSBB.Email = user.Email;
                addTempOSBB.OSBBName = myOSBB.OSBBs.FirstOrDefault(o => o.Id == addTempOSBB.IdOSBB).Name;
                addTempOSBB.Action = OSBBActions.add.ToString();
                myOSBB.AddTempOSBBs.Add(addTempOSBB);
                myOSBB.SaveChanges();
                return RedirectToAction("UserProfile", "Main");
            }
            else
            {
                RedirectToAction("MyOSBBAdd", "Main" );
            }

            return View();
        }

        // Заявка на видалення
        [MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult RejectTempApartment(string selectedOSBBIdInDropDown, string deletedIdOSBB, string deletedIdApartment)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			int idSelectedOSBB = Int32.Parse(selectedOSBBIdInDropDown);
            int idDeleteOSBB = Int32.Parse(deletedIdOSBB);
            int idDeletedApartment = Int32.Parse(deletedIdApartment);
			AddTempOSBB userAddTempOSBB;
			UserOSBBApartment userOSBBApartment = myOSBB.UserOSBBApartments.FirstOrDefault(s => s.IdOSBB == idDeleteOSBB && s.ApartmentNumber == idDeletedApartment && s.IdUser == user.Id);
			if (userOSBBApartment != null)
			{
				userAddTempOSBB = new AddTempOSBB
				{
					IdUser = user.Id,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					MiddleName = user.MiddleName,
					IdOSBB = userOSBBApartment.IdOSBB,
					OSBBName = myOSBB.OSBBs.FirstOrDefault(s => s.Id == userOSBBApartment.IdOSBB).Name,
					ApartmentNumber = userOSBBApartment.ApartmentNumber,
					Action = OSBBActions.delete.ToString()
				};
				myOSBB.AddTempOSBBs.Add(userAddTempOSBB);
				//myOSBB.UserOSBBApartments.Remove(userOSBBApartment);
				myOSBB.SaveChanges();
			}

			return RedirectToAction("UserProfile", "Main", new { id = selectedOSBBIdInDropDown });
		}

        // Підтвердження квартири
        [MyFilter(Roles = "ROLE_HEAD")]
		public ActionResult AddTempApartmentToMain(string selectedOSBBIdInDropDown, string addedId, string userId, string apartmentNumber)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			int idSelectedOSBB = Int32.Parse(selectedOSBBIdInDropDown);
			int idAddedOSBB = Int32.Parse(addedId);
			int idUser = Int32.Parse(userId);
			int apartmentNum = Int32.Parse(apartmentNumber);
			UserOSBBApartment userOSBBApartment;
			AddTempOSBB userAddTempOSBB;
			if (myOSBB.OSBBs.FirstOrDefault(s => s.IdHead == user.Id && s.Id == idSelectedOSBB) != null)
			{
				userAddTempOSBB = myOSBB.AddTempOSBBs.FirstOrDefault(s => s.Id == idAddedOSBB);
				userOSBBApartment = new UserOSBBApartment
				{
					IdOSBB = userAddTempOSBB.IdOSBB,
					IdUser = idUser,
					ApartmentNumber = apartmentNum
				};
                for(int i = 0; i < 3; i++)
                {
                    myOSBB.UserAccountServices.Add(new UserAccountService
                    {
                        IdUser = user.Id,
                        IdOSBB = userAddTempOSBB.IdOSBB,
                        ApartmentNumber = apartmentNum,
                        IdTypeServices = i + 1,
                        AccountNumber = "-"
                    });
                }
				myOSBB.UserOSBBApartments.Add(userOSBBApartment);
				myOSBB.AddTempOSBBs.Remove(userAddTempOSBB);
				myOSBB.SaveChanges();

			}
			return RedirectToAction("MyOSBBSettings", "Main", new { id = selectedOSBBIdInDropDown });
		}

        // Відхилення квартири
		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult DeleteTempApartment(string page, string selectedOSBBIdInDropDown, string deletedIdOSBB, string deletedIdApartmentNumber)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			int idDeleteOSBB = Int32.Parse(deletedIdOSBB);
			int idSelectedOSBB = Int32.Parse(selectedOSBBIdInDropDown);
            int idSelectedApartmentNumber = Int32.Parse(deletedIdApartmentNumber);

   //         AddTempOSBB userAddTempOSBB;
			//if (myOSBB.OSBBs.FirstOrDefault(s => s.IdHead == user.Id && s.Id == idSelectedOSBB) != null)
			//{
			//	userAddTempOSBB = myOSBB.AddTempOSBBs.FirstOrDefault(s => s.Id == idDeleteOSBB);
			//}
			//else
			//{
			//	userAddTempOSBB = myOSBB.AddTempOSBBs.FirstOrDefault(s => s.Id == idDeleteOSBB && s.IdUser == user.Id);
			//}
            if(selectedOSBBIdInDropDown != null && idDeleteOSBB != 0 && idSelectedOSBB != 0 && idSelectedApartmentNumber != 0)
            {
                AddTempOSBB userAddTempOSBB = myOSBB.AddTempOSBBs.FirstOrDefault(u => u.IdUser == user.Id && u.IdOSBB == idDeleteOSBB && u.ApartmentNumber == idSelectedApartmentNumber);
                if (userAddTempOSBB != null)
                {
                    UserOSBBApartment userOSBBApartment = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id && u.IdOSBB == idDeleteOSBB && u.ApartmentNumber == idSelectedApartmentNumber);
                    //myOSBB.UserOSBBApartments.Remove(userOSBBApartment);
                    myOSBB.AddTempOSBBs.Remove(userAddTempOSBB);
                    myOSBB.SaveChanges();
                }
                return RedirectToAction(page, "Main", new { id = selectedOSBBIdInDropDown });

            }
            var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
                             from OSBB in myOSBB.OSBBs
                             where ((UserOSBBApartment.IdUser == user.Id) &&
                                    (OSBB.Id == UserOSBBApartment.IdOSBB))
                             select new
                             {
                                 osbbID = UserOSBBApartment.IdOSBB
                             }
            ).Distinct().ToList();

            foreach (var osbb in userOSBBs)
            {
                osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
            }
            return RedirectToAction("MyOSBB", "Main", new { id = osbbVal[0].Id.ToString() });
        }

        // Остаточне видалення квартири
        [MyFilter(Roles = "ROLE_HEAD")]
        public ActionResult DeleteApartment(string selectedOSBBIdInDropDown, string deletedIdOSBB, string deletedIdApartmentNumber)
        {
            User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            int idDeleteOSBB = Int32.Parse(deletedIdOSBB);
            int idSelectedOSBB = Int32.Parse(selectedOSBBIdInDropDown);
            int idSelectedApartmentNumber = Int32.Parse(deletedIdApartmentNumber);

            if (selectedOSBBIdInDropDown != null && idDeleteOSBB != 0 && idSelectedOSBB != 0 && idSelectedApartmentNumber != 0)
            {
                AddTempOSBB userAddTempOSBB = myOSBB.AddTempOSBBs.FirstOrDefault(u => u.IdUser == user.Id && u.IdOSBB == idDeleteOSBB && u.ApartmentNumber == idSelectedApartmentNumber);
                if (userAddTempOSBB != null)
                {
                    UserOSBBApartment userOSBBApartment = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id && u.IdOSBB == userAddTempOSBB.IdOSBB 
                                                                                                            && u.ApartmentNumber == userAddTempOSBB.ApartmentNumber);
                    List<UserAccountService> userAccountServices = (from UserAccountServices in myOSBB.UserAccountServices
                                                                   where UserAccountServices.IdUser == user.Id && UserAccountServices.ApartmentNumber == idSelectedApartmentNumber
                                                                   select UserAccountServices).ToList();
                    foreach(var userAccountService in userAccountServices)
                    {
                        myOSBB.UserAccountServices.Remove(userAccountService);
                    }
                    myOSBB.UserOSBBApartments.Remove(userOSBBApartment);
                    myOSBB.AddTempOSBBs.Remove(userAddTempOSBB);
                    myOSBB.SaveChanges();
                }
                return RedirectToAction("MyOSBBSettings", "Main", new { id = selectedOSBBIdInDropDown });
            }
            var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
                             from OSBB in myOSBB.OSBBs
                             where ((UserOSBBApartment.IdUser == user.Id) &&
                                    (OSBB.Id == UserOSBBApartment.IdOSBB))
                             select new
                             {
                                 osbbID = UserOSBBApartment.IdOSBB
                             }
            ).Distinct().ToList();

            foreach (var osbb in userOSBBs)
            {
                osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
            }
            return RedirectToAction("MyOSBBSettings", "Main", new { id = osbbVal[0].Id.ToString() });
        }

		[HttpPost]
		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult ChangeUserServicesNumbers(ProfileViewModel profileViewModel)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			if(profileViewModel != null && profileViewModel.changeUserServicesNumbersModel.IdOSBB != 0 
											&& profileViewModel.changeUserServicesNumbersModel.ApartmentNumber != 0 
											&& profileViewModel.changeUserServicesNumbersModel.GazNumber != null
											&& profileViewModel.changeUserServicesNumbersModel.WaterNumber != null
											&& profileViewModel.changeUserServicesNumbersModel.ElectricityNumber != null)
			{
				List<UserAccountService> oldUserAccountService = new List<UserAccountService>();
				for(int i = 0; i < 3; i++)
				{
					oldUserAccountService.Add(myOSBB.UserAccountServices.FirstOrDefault(
					u => u.IdOSBB == profileViewModel.changeUserServicesNumbersModel.IdOSBB
					&& u.IdUser == user.Id && u.ApartmentNumber == profileViewModel.changeUserServicesNumbersModel.ApartmentNumber && u.IdTypeServices == i + 1));
					
				}
				if(oldUserAccountService != null)
				{
					for (int i = 0; i < 3; i++)
					{
						UserAccountService tempUserAccountService = oldUserAccountService[i];
						if (tempUserAccountService.IdTypeServices == 1)
						{
							tempUserAccountService.AccountNumber = profileViewModel.changeUserServicesNumbersModel.ElectricityNumber;
							myOSBB.Entry(oldUserAccountService[i]).CurrentValues.SetValues(tempUserAccountService);
						}
						if (tempUserAccountService.IdTypeServices == 2)
						{
							tempUserAccountService.AccountNumber = profileViewModel.changeUserServicesNumbersModel.WaterNumber;
							myOSBB.Entry(oldUserAccountService[i]).CurrentValues.SetValues(tempUserAccountService);
						}
						if (tempUserAccountService.IdTypeServices == 3)
						{
							tempUserAccountService.AccountNumber = profileViewModel.changeUserServicesNumbersModel.GazNumber;
							myOSBB.Entry(oldUserAccountService[i]).CurrentValues.SetValues(tempUserAccountService);
						}
					}
					myOSBB.SaveChanges();
					//UserAccountService newuUserAccountService = new UserAccountService
					//{
					//	Id = userAccountService.Id,

					//	IdUser = userAccountService.IdUser,

					//	IdOSBB = userAccountService.IdOSBB,

					//	ApartmentNumber =;

					//	IdTypeServices =;

					//	AccountNumber =;
					//}; myOSBB.Entry(userAccountService).CurrentValues.SetValues(newuUserAccountService);
				}
			}


			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
					).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}
			return RedirectToAction("UserProfile", "Main", new { id = osbbVal[0].Id.ToString() });
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult MyOSBBSettings(string id)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;
			int idosbb;
			CreateMenu(3);
			ViewBag.PageItemsList = pageItemsList;
			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
					).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			if (Int32.TryParse(id, out idosbb))
			{
                selectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idosbb);
                
                if (selectedOSBB.IdHead == user.Id)
				{
                    ViewBag.SelectedOSBB = selectedOSBB;
                    OSBBSettingsModel osbbSettingsModel = new OSBBSettingsModel();
                    osbbSettingsModel.selectedOSBB = selectedOSBB;
                    osbbSettingsModel.OSBBList = osbbVal;
                    ViewBag.OSBBUsers = (from UserOSBBApartments in myOSBB.UserOSBBApartments
                                         from Users in myOSBB.Users
                                         where UserOSBBApartments.IdOSBB == selectedOSBB.Id && UserOSBBApartments.IdUser == Users.Id
                                         select new OSBBSettingsUsersModel
                                         {
                                             ApartmentNumber = UserOSBBApartments.ApartmentNumber,
                                             UserName = Users.LastName + " " + Users.FirstName + " " + Users.MiddleName,
                                         }).ToList();
                    ViewBag.OSBBAccountServices = (from OSBBAccountServices in myOSBB.OSBBAccountServices
                                                   where OSBBAccountServices.IdOSBB == selectedOSBB.Id
                                                   select OSBBAccountServices).ToList();
                    ViewBag.OSBBTypeServices = (from OSBBAccountServices in myOSBB.OSBBAccountServices
                                                   from AccountTypeServices in myOSBB.AccountTypeServices
                                                   where AccountTypeServices.Id == OSBBAccountServices.IdType
                                                   select AccountTypeServices).Distinct().ToList();
					ViewBag.AddTempOSBB = (from UserOSBBApartment in myOSBB.UserOSBBApartments
										   from AddTempOSBBs in myOSBB.AddTempOSBBs
										   where AddTempOSBBs.IdUser == user.Id && AddTempOSBBs.IdOSBB == idosbb
										   select AddTempOSBBs).Distinct().ToList();

					return View(osbbSettingsModel);
				}
				else
				{
					return RedirectToAction("Main", "Main", new { id = osbbVal[0].Id });
				}
			}
			else
			{
				return RedirectToAction("Main", "Main", new { id = osbbVal[0].Id });
			}

		}

		[HttpPost]
		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult UpdateOSBBName(OSBBSettingsModel model)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			if (model != null)
			{
				OSBB osbbToUpdate = myOSBB.OSBBs.FirstOrDefault(o => o.Id == model.selectedOSBB.Id);
				if (osbbToUpdate != null && osbbToUpdate.IdHead == user.Id)
				{
					OSBB newOSBB = osbbToUpdate;
					newOSBB.Name = model.selectedOSBB.Name;
					myOSBB.Entry(osbbToUpdate).CurrentValues.SetValues(newOSBB);
					myOSBB.SaveChanges();
					return RedirectToAction("MyOSBBSettings", "Main", new { id = osbbToUpdate.Id });
				}
			}

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
					).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			return RedirectToAction("MyOSBBSettings", "Main", new { id = osbbVal[0].Id });
		}


        [HttpPost]
        [MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
        public ActionResult ChangeUserPassword(ProfileViewModel model)
        {
            User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            string errorStr = null;




            if(!Crypto.VerifyHashedPassword(user.Password, model.changePasswordModel.OldPassword) 
                && myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id && u.IdOSBB == model.changePasswordModel.IdOSBB) != null)
            {
                errorStr = "Невдалось змінити пароль. Старий пароль невірний";
                return RedirectToAction("UserProfile", "Main", new { id = model.changePasswordModel.IdOSBB.ToString(), error = errorStr });
            }
            if (Crypto.VerifyHashedPassword(user.Password, model.changePasswordModel.OldPassword) 
					&& model.changePasswordModel.NewPassword == model.changePasswordModel.NewPasswordConfirm 
					&& myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id && u.IdOSBB == model.changePasswordModel.IdOSBB) != null)
            {
                User newUserData = user;
                newUserData.Password = Crypto.HashPassword(model.changePasswordModel.NewPassword);
                myOSBB.Entry(user).CurrentValues.SetValues(newUserData);
                myOSBB.SaveChanges();
                return RedirectToAction("UserProfile", "Main", new { id = model.changePasswordModel.IdOSBB.ToString(), error = errorStr });
            }





            var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
                             from OSBB in myOSBB.OSBBs
                             where ((UserOSBBApartment.IdUser == user.Id) &&
                                    (OSBB.Id == UserOSBBApartment.IdOSBB))
                             select new
                             {
                                 osbbID = UserOSBBApartment.IdOSBB
                             }
            ).Distinct().ToList();

            foreach (var osbb in userOSBBs)
            {
                osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
            }
            return RedirectToAction("UserProfile", "Main", new { id = osbbVal[0].Id.ToString(), error = errorStr });

        }

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public JsonResult getHashPassword(string oldpassword1, string oldpassword2)
		{
			if (Crypto.VerifyHashedPassword(oldpassword1, oldpassword2)) { 
				return Json(1, JsonRequestBehavior.AllowGet);
			}
			return Json(0, JsonRequestBehavior.AllowGet);

		}

		[HttpPost]
		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult UpdateAccountNumber(OSBBSettingsModel model)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			if(model.addOSBBAccountServiceModel != null)
			{
				OSBB osbb = myOSBB.OSBBs.FirstOrDefault(o => o.Id == model.addOSBBAccountServiceModel.IdOSBB);
				AccountTypeService accountTypeService  = new AccountTypeService();
				accountTypeService.Name = model.addOSBBAccountServiceModel.AccountTypeName;
				myOSBB.AccountTypeServices.Add(accountTypeService);
				myOSBB.SaveChanges();

				int accountTypeId = myOSBB.AccountTypeServices.FirstOrDefault(a => a.Name == model.addOSBBAccountServiceModel.AccountTypeName).Id;
				if (accountTypeId != 0 && osbb != null && osbb.IdHead == user.Id)
				{
					OSBBAccountService osbbAccountService = new OSBBAccountService
					{
						IdOSBB = model.addOSBBAccountServiceModel.IdOSBB,
						IdType = accountTypeId,
						AccountNumber = model.addOSBBAccountServiceModel.AccountNumber
					};
					myOSBB.OSBBAccountServices.Add(osbbAccountService);
					myOSBB.SaveChanges();
					return RedirectToAction("MyOSBBSettings", "Main", new { id = osbbAccountService.IdOSBB });
				}
			}
			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
					).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			return RedirectToAction("MyOSBBSettings", "Main", new { id = osbbVal[0].Id });
		}

        [HttpPost]
        [MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
        public ActionResult DeleteAccountNumber(OSBBSettingsModel model)
        {
            User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
            if (model.osbbAccountService != null)
            {
                OSBBAccountService osbbAccountService = myOSBB.OSBBAccountServices.FirstOrDefault(a => a.Id == model.osbbAccountService.Id);
                if (osbbAccountService != null)
                {
                    OSBB osbb = myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbbAccountService.IdOSBB && o.IdHead == user.Id);
                    if(osbb != null)
                    {
                        myOSBB.OSBBAccountServices.Remove(osbbAccountService);
                        myOSBB.SaveChanges();
                        return RedirectToAction("MyOSBBSettings", "Main", new { id = osbbAccountService.IdOSBB });
                    }
                }
            }
            var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
                             from OSBB in myOSBB.OSBBs
                             where ((UserOSBBApartment.IdUser == user.Id) &&
                                    (OSBB.Id == UserOSBBApartment.IdOSBB))
                             select new
                             {
                                 osbbID = UserOSBBApartment.IdOSBB
                             }
                    ).Distinct().ToList();

            foreach (var osbb in userOSBBs)
            {
                osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
            }
            return RedirectToAction("MyOSBBSettings", "Main", new { id = osbbVal[0].Id });
        }


        [MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult Payment(string selectedOSBBIdInDropDown, string apartmentNumber)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;
			ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);
			int idSelectedOSBB = Int32.Parse(selectedOSBBIdInDropDown);
			int apartmentNum = Int32.Parse(apartmentNumber);

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			CreateMenu(2);

			ViewBag.PageItemsList = pageItemsList;


			if (selectedOSBBIdInDropDown == null || apartmentNumber == null || idSelectedOSBB == 0 || apartmentNum == 0)
			{
				if (osbbVal.Count() == 0)
				{
					ViewBag.SelectedOSBB = new OSBB();
					return RedirectToAction("Main", "Main", new { id = osbbVal[0].Id });
				}
				else
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					selectedOSBB = osbbVal[0];
					return RedirectToAction("MyOSBB", "Main", new { id = osbbVal[0].Id });
				}
			}
			else
			{
				if (userOSBBs.FirstOrDefault(o => o.osbbID == idSelectedOSBB) != null)
				{
					ViewBag.SelectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idSelectedOSBB);
					selectedOSBB = ViewBag.SelectedOSBB;
					ViewBag.UserAccountServices = (from UserAccountServices in myOSBB.UserAccountServices
												   where UserAccountServices.IdUser == user.Id && UserAccountServices.IdOSBB == idSelectedOSBB && UserAccountServices.ApartmentNumber == apartmentNum
												   select UserAccountServices).Distinct().ToList();

					ViewBag.AccountTypeServices = (from UserAccountServices in myOSBB.UserAccountServices
												   from OSBBAccountServices in myOSBB.OSBBAccountServices
												   from AccountTypeServices in myOSBB.AccountTypeServices
												   where UserAccountServices.IdUser == user.Id && AccountTypeServices.Id == UserAccountServices.IdTypeServices 
												   || AccountTypeServices.Id  == OSBBAccountServices.IdType
												   select AccountTypeServices).Distinct().ToList();
					ViewBag.OSBBAccountServices = (from OSBBAccountServices in myOSBB.OSBBAccountServices
												   from AccountTypeServices in myOSBB.AccountTypeServices
												   where OSBBAccountServices.IdOSBB == selectedOSBB.Id
												   select OSBBAccountServices).Distinct().ToList();
					List<PayModel> payModel = new List<PayModel>();
					string private_key = "0nwsW2izQOXKS4fQnGuUyfH15VPx2SgxyBTrPSUX";
					foreach (var service in ViewBag.UserAccountServices)
					{
						foreach (var type in ViewBag.AccountTypeServices)
						{
							if (service.IdTypeServices == type.Id)
							{
								string value
									= "{ \"public_key\":\"i16489745472\",\"version\":\"3\",\"action\":\"pay\",\"amount\":\"5\",\"currency\":\"UAH\",\"description\":\"test\",\"order_id\":\"000001\"}";
								string data = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
								string signature = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(private_key + data + private_key)));
								payModel.Add(new PayModel
								{
									IdUser = user.Id,

									IdOSBB = selectedOSBB.Id,

									ApartmentNumber = service.ApartmentNumber,

									TypeServicesName = type.Name,

									AccountNumber = service.AccountNumber,

									PayButton = string.Format(@"<form method='POST' action='https://www.liqpay.ua/api/3/checkout' accept-charset='utf-8'> 
									 <input type='hidden' name='data' value = {0} />
									 <input type='hidden' name='signature' value = {1} />
									 
										<button style='border: none !important; display: inline - block !important; text - align: center !important; padding: 7px 20px !important;
											color: #fff !important; font-size:16px !important; font-weight: 600 !important; font-family:OpenSans, sans-serif; cursor: pointer !important; border-radius: 2px !important;
												background: rgb(122, 183, 43) !important; 'onmouseover='this.style.opacity = '0.5'; 'onmouseout='this.style.opacity = '1'; '>
											<img src = 'https://static.liqpay.ua/buttons/logo-small.png' name = 'btn_text' 
												style = 'margin-right: 7px !important; vertical-align: middle !important;' />
											<span style='vertical-align:middle; !important'> Сплатити </ span >
	 
										</button >
	 
									  </form>", data, signature)
								});

							}

						}

					}

					List<PayModel> payModel2 = new List<PayModel>();
					foreach (var osbb in ViewBag.OSBBAccountServices)
					{
						foreach (var type in ViewBag.AccountTypeServices)
						{
							if (osbb.IdType == type.Id)
							{
								string value
									= "{ \"public_key\":\"i16489745472\",\"version\":\"3\",\"action\":\"pay\",\"amount\":\"5\",\"currency\":\"UAH\",\"description\":\"" + type.Name + "\",\"order_id\":\"000001\"}";
								string data = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
								string signature = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(private_key + data + private_key)));
								payModel2.Add(new PayModel
								{
									IdUser = user.Id,

									IdOSBB = selectedOSBB.Id,

									ApartmentNumber = -1,

									TypeServicesName = type.Name,

									AccountNumber = osbb.AccountNumber,

									PayButton = string.Format(@"<form method='POST' action='https://www.liqpay.ua/api/3/checkout' accept-charset='utf-8'> 
									 <input type='hidden' name='data' value = {0} />
									 <input type='hidden' name='signature' value = {1} />
									 
										<button style='border: none !important; display: inline - block !important; text - align: center !important; padding: 7px 20px !important;
											color: #fff !important; font-size:16px !important; font-weight: 600 !important; font-family:OpenSans, sans-serif; cursor: pointer !important; border-radius: 2px !important;
												background: rgb(122, 183, 43) !important; 'onmouseover='this.style.opacity = '0.5'; 'onmouseout='this.style.opacity = '1'; '>
											<img src = 'https://static.liqpay.ua/buttons/logo-small.png' name = 'btn_text' 
												style = 'margin-right: 7px !important; vertical-align: middle !important;' />
											<span style='vertical-align:middle; !important'> Сплатити </ span >
	 
										</button >
	 
									  </form>", data, signature)
								});

							}

						}
					}
					ViewBag.PayModel = payModel;
					ViewBag.PayModel2 = payModel2;
					//ViewData["pay"] = string.Format(@"<form method='POST' action='https://www.liqpay.ua/api/3/checkout' accept-charset='utf-8'> 
					// < input type = 'hidden' name = 'data' value = {0} />
					// < input type = 'hidden' name = 'signature' value = {1} />
					// < input type = 'image' src = '//static.liqpay.ua/buttons/p1ru.radius.png' />
					//</ form > ");    <input type='image' src='https://static.liqpay.ua/buttons/logo-small.png' />
					return View(osbbVal);
				}
				else
				{
					return RedirectToAction("Main", "Main", new { id = osbbVal[0].Id });
				}
				////ViewBag.SelectedOSBB = osbbVal.Select(o => o.Name = Request.Form["osbbList"].ToString());
				//ViewBag.SelectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idSelectedOSBB);
				//selectedOSBB = ViewBag.SelectedOSBB;
				//return View(osbbVal);
			}

			//return View();
		}

		public ActionResult Chat(string id, string userPar, bool? logOn, bool? logOff, string chatMessage)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;
			ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			CreateMenu(4);

			ViewBag.PageItemsList = pageItemsList;

			if (id == null || Int32.Parse(id) == 0)
			{
				if (osbbVal.Count() == 0)
				{
					ViewBag.AllOSBBUsers = new List<User>();
					ViewBag.SelectedOSBB = new OSBB();
                    ChatModel chatModel = new ChatModel
                    {
                        OSBBVal = osbbVal,
                        Messages = null
                    };
                    return View(chatModel);
				}
				if (selectedOSBB != null)
				{
					ViewBag.AllOSBBUsers = new List<User>();
					ViewBag.SelectedOSBB = selectedOSBB;
					//if (ViewBag.SelectedOSBB == null)
					//{
					//    ViewBag.SelectedOSBB = osbbVal[0];
					//    if (osbbVal[0].IdHead == user.Id)
					//    {
					//        CreateMenu(4);
					//    }
					//    else
					//    {
					//        CreateMenu(3);
					//    }
					//}
					return View(osbbVal);
				}
				else
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					ViewBag.AllOSBBUsers = (from Users in myOSBB.Users
											from UserOSBBApartments in myOSBB.UserOSBBApartments
											where Users.Id == UserOSBBApartments.Id && UserOSBBApartments.IdOSBB == osbbVal[0].Id
											select Users).ToList();
					selectedOSBB = osbbVal[0];
					//if (osbbVal[0].IdHead == user.Id)
					//{
					//    CreateMenu(4);
					//}
					//else
					//{
					//    CreateMenu(3);
					//}
					return View(osbbVal);
				}
			}
			else
			{
				int idosbb = Int32.Parse(id);
				//ViewBag.SelectedOSBB = osbbVal.Select(o => o.Name = Request.Form["osbbList"].ToString());
				ViewBag.SelectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idosbb);

				if (ViewBag.SelectedOSBB == null)
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					ViewBag.AllOSBBUsers = (from Users in myOSBB.Users
											from UserOSBBApartments in myOSBB.UserOSBBApartments
											where Users.Id == UserOSBBApartments.IdUser && UserOSBBApartments.IdOSBB == osbbVal[0].Id
											select Users).Distinct().ToList();
					ViewBag.AllOSBBUsers = (from Users in myOSBB.Users
											from UserOSBBApartments in myOSBB.UserOSBBApartments
											where Users.Id == UserOSBBApartments.IdUser && UserOSBBApartments.IdOSBB == osbbVal[0].Id
											select Users).Distinct().ToList();

					ViewBag.Messages = (from Messages in myOSBB.Messages
										where Messages.IdOSBB == osbbVal[0].Id
										orderby Messages.MessageDate ascending
										select Messages)/*.Take(15)*/.ToList();
					//if (osbbVal[0].IdHead == user.Id)
					//{
					//    CreateMenu(4);
					//}
					//else
					//{
					//    CreateMenu(3);
					//}
				}
				else
				{
					ViewBag.AllOSBBUsers = (from Users in myOSBB.Users
											from UserOSBBApartments in myOSBB.UserOSBBApartments
											where Users.Id == UserOSBBApartments.IdUser && UserOSBBApartments.IdOSBB == idosbb
											select Users).Distinct().ToList();
					var messages = (from Messages in myOSBB.Messages
									where Messages.IdOSBB == idosbb
									orderby Messages.MessageDate descending
									select Messages)/*.Take(15)*/.ToList();
					//if (osbbVal[0].IdHead == user.Id)
					//{
					//    CreateMenu(4);
					//}
					//else
					//{
					//    CreateMenu(3);
					//}
					try
					{

						ChatModel chatModel = new ChatModel
						{
							OSBBVal = osbbVal,
							Messages = messages
						};
						// если обычный запрос, просто возвращаем представление
						if (!Request.IsAjaxRequest())
						{
							return View(chatModel);
						}
						else
						{
							// добавляем в список сообщений новое сообщение
							if (!string.IsNullOrEmpty(chatMessage))
							{
								myOSBB.Messages.Add(new Message
								{
									IdOSBB = selectedOSBB.Id,
									User = userPar,
									MessageDate = DateTime.Now,
									Text = chatMessage
								});
							}

							messages = (from Messages in myOSBB.Messages
										where Messages.IdOSBB == idosbb
										orderby Messages.MessageDate descending
										select Messages)/*.Take(15)*/.ToList();

							return PartialView("History", messages);
						}
					}
					catch (Exception ex)
					{
						//в случае ошибки посылаем статусный код 500
						Response.StatusCode = 500;
						return Content(ex.Message);
					}

				}
				if (ViewBag.SelectedOSBB.IdHead == user.Id)
				{
					CreateMenu(4);
				}
				else
				{
					CreateMenu(3);
				}
				selectedOSBB = ViewBag.SelectedOSBB;
				return View(osbbVal);
			}

		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		[Route("contact/conversations")]
		public JsonResult ConversationWithContact(int contact)
		{

			var currentUser = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);

			var conversations = new List<Models.Conversation>();

			conversations = myOSBB.Conversations.
								  Where(c => (c.IdReceiver == currentUser.Id
									  && c.IdSender == contact) ||
									  (c.IdReceiver == contact
									  && c.IdSender == currentUser.Id))
								  .OrderBy(c => c.CreatedAt)
								  .ToList();

			return Json(
				new { status = "success", data = conversations },
				JsonRequestBehavior.AllowGet
			);
		}

		[HttpPost]
		[Route("send_message")]
		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public JsonResult SendMessage()
		{

			var currentUser = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			int id = Int32.Parse(Request.Form["idosbb"]);
			//string socket_id = Request.Form["socket_id"];

			Message message = new Message
			{
				IdOSBB = id,
				User = Request.Form["user"],
				MessageDate = DateTime.Now,
				Text = Request.Form["message"]
			};
			//Conversation conversation = new Conversation
			//{
			//    IdSender = currentUser.Id,
			//    Message = Request.Form["message"],
			//    IdReceiver = Convert.ToInt32(Request.Form["contact"]),
			//    CreatedAt = DateTime.Now
			//};



			//myOSBB.Conversations.Add(conversation);
			myOSBB.Messages.Add(message);
			myOSBB.SaveChanges();

			//       var conversationChannel = getConvoChannel(currentUser.Id, conversation.IdReceiver);

			//       pusher.TriggerAsync(
			//         conversationChannel,
			//         "new_message",
			//message,
			//         new TriggerOptions() { SocketId = socket_id });

			return Json(message);
		}

		private String getConvoChannel(int user_id, int contact_id)
		{
			if (user_id > contact_id)
			{
				return "private-chat-" + contact_id + "-" + user_id;
			}

			return "private-chat-" + user_id + "-" + contact_id;
		}

		[Route("pusher/auth")]
		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public JsonResult AuthForChannel(string channel_name, string socket_id)
		{

			var currentUser = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);

			var options = new PusherOptions();
			options.Cluster = "PUSHER_APP_CLUSTER";

			var pusher = new Pusher(
			"522229",
			"e9bcc3a4af8977d151f5",
			"aa53148f11e931b0436b", options);

			if (channel_name.IndexOf(currentUser.Id.ToString()) == -1)
			{
				return Json(
				  new { status = "error", message = "User cannot join channel" }
				);
			}

			var auth = pusher.Authenticate(channel_name, socket_id);

			return Json(auth);
		}



		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult News(string id)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;
			ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}



			CreateMenu(5);
			ViewBag.PageItemsList = pageItemsList;


			if (id == null || Int32.Parse(id) == 0)
			{
				if (osbbVal.Count() == 0)
				{
					ViewBag.SelectedOSBB = new OSBB();
					return View(osbbVal);
				}
				if (selectedOSBB != null)
				{
					ViewBag.SelectedOSBB = selectedOSBB;
					ViewBag.News = (from News in myOSBB.News
									where News.IdOSBB == selectedOSBB.Id
									orderby News.Id descending
									select News).ToList();
					return View(osbbVal);
				}
				else
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					selectedOSBB = osbbVal[0];
					ViewBag.News = (from News in myOSBB.News
									where News.IdOSBB == selectedOSBB.Id
									orderby News.Id descending
									select News).ToList();
					return View(osbbVal);
				}
			}
			else
			{
				int idosbb = Int32.Parse(id);
				ViewBag.SelectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idosbb);
				if (ViewBag.SelectedOSBB == null)
				{
					if (myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdOSBB == idosbb && u.IdUser == user.Id) != null)
					{
						ViewBag.SelectedOSBB = osbbVal[0];
						ViewBag.News = (from News in myOSBB.News
										where News.IdOSBB == osbbVal[0].Id
										orderby News.Id descending
										select News).ToList();
					}
					else
					{
						return RedirectToAction("News", "Main", new { id = osbbVal[0].Id });
					}
				}
				selectedOSBB = ViewBag.SelectedOSBB;
				ViewBag.News = (from News in myOSBB.News
								where News.IdOSBB == selectedOSBB.Id
								orderby News.Id descending
								select News).ToList();
				return View(osbbVal);
			}
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult New(string selectedOSBBIdInDropDown, string selectedNews)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;
			ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}


			CreateMenu(5);
			ViewBag.PageItemsList = pageItemsList;

			int idSelectedNews;
			if (selectedOSBBIdInDropDown == null || Int32.Parse(selectedOSBBIdInDropDown) == 0)
			{
				if (osbbVal.Count() == 0)
				{
					ViewBag.SelectedOSBB = new OSBB();
					return RedirectToAction("News", "Main");
				}
				if (selectedOSBB != null)
				{
					ViewBag.SelectedOSBB = selectedOSBB;
					if (Int32.TryParse(selectedNews, out idSelectedNews))
					{
						ViewBag.New = myOSBB.News.FirstOrDefault(s => s.Id == idSelectedNews && s.IdOSBB == selectedOSBB.Id);
					}

					return View(osbbVal);
				}
				else
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					selectedOSBB = osbbVal[0];

					if (Int32.TryParse(selectedNews, out idSelectedNews))
					{
						ViewBag.New = myOSBB.News.FirstOrDefault(s => s.Id == idSelectedNews && s.IdOSBB == osbbVal[0].Id);
					}
					return RedirectToAction("News", "Main", new { id = osbbVal[0].Id });
				}
			}
			else
			{
				int idosbb = Int32.Parse(selectedOSBBIdInDropDown);
				ViewBag.SelectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idosbb);
				if (ViewBag.SelectedOSBB == null)
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					ViewBag.News = (from News in myOSBB.News
									where News.IdOSBB == osbbVal[0].Id
									orderby News.Date ascending
									select News).ToList();
				}
				selectedOSBB = ViewBag.SelectedOSBB;

				if (Int32.TryParse(selectedNews, out idSelectedNews))
				{
					ViewBag.New = myOSBB.News.FirstOrDefault(s => s.Id == idSelectedNews && s.IdOSBB == selectedOSBB.Id);
				}
				return View(osbbVal);
			}
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult NewAdd(string selectedOSBBIdInDropDown)
		{
			int idSelectedOSBB;
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
					).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			CreateMenu(5);

			if (Int32.TryParse(selectedOSBBIdInDropDown, out idSelectedOSBB))
			{
				if (myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdOSBB == idSelectedOSBB && u.IdUser == user.Id) != null)
				{
					ViewBag.User = user;
					ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);
					ViewBag.SelectedOSBB = myOSBB.OSBBs.FirstOrDefault(u => u.Id == idSelectedOSBB);
					//var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
					//                 from OSBB in myOSBB.OSBBs
					//                 where ((UserOSBBApartment.IdUser == user.Id) &&
					//                        (OSBB.Id == UserOSBBApartment.IdOSBB))
					//                 select new
					//                 {
					//                     osbbID = UserOSBBApartment.IdOSBB
					//                 }
					//).Distinct().ToList();

					foreach (var osbb in userOSBBs)
					{
						osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
					}
					AddNewModel addNewModel = new AddNewModel { newObj = new New(), OSBBList = osbbVal };

					ViewBag.PageItemsList = pageItemsList;
					return View(addNewModel);

				}
				else
				{
					ViewBag.PageItemsList = pageItemsList;

					return RedirectToAction("News", "Main",
						new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).ToString() });
				}
			}
			else
			{
				return RedirectToAction("News", "Main",
					new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).ToString() });
			}
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult AddNew(AddNewModel model, HttpPostedFileBase upload)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			model.newObj.Author = (user.LastName + " " + user.FirstName + " " + user.MiddleName).ToString();
			model.newObj.Date = DateTime.Now.Date;
			if (upload != null)
			{
				string fileName = System.IO.Path.GetFileName(upload.FileName);
				upload.SaveAs(Server.MapPath("~/Content/images/news/" + fileName));
				model.newObj.PathToPhoto = "/Content/images/news/" + fileName;
				myOSBB.News.Add(model.newObj);
			}
			else
			{
				model.newObj.PathToPhoto = "/Content/images/news/defaultNew.png";
				myOSBB.News.Add(model.newObj);
			}

			myOSBB.SaveChanges();
			EmailService emailService = new EmailService();

			var userList = (from Users in myOSBB.Users
							from UserOSBBApartments in myOSBB.UserOSBBApartments
							where Users.Id == UserOSBBApartments.IdUser && UserOSBBApartments.IdOSBB == model.newObj.IdOSBB
							select Users).Distinct().ToList();
			foreach (var userV in userList)
			{
				var url = Url.Action("New", "Main",
					new { selectedOSBBIdInDropDown = model.newObj.IdOSBB, selectedNews = model.newObj.Id }, protocol: Request.Url.Scheme);
				emailService.SendEmail(userV.Email, "Новина - " + model.newObj.Title,
				"Більш детальніше на " + url + "\"");
			}
				//emailService.SendEmail(user.Email, "Новина - " + model.newObj.Title,
				//"Більш детальніше на http://kappa123kappa123-001-site1.btempurl.com/Main/News/" + model.newObj.IdOSBB + "/" + model.newObj.Id);
			return RedirectToAction("News", "Main",
					new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).IdOSBB.ToString() });
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult NewEdit(string selectedOSBBIdInDropDown, string selectedNews)
		{
			int idSelectedOSBB;
			int idNew;
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
					).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			CreateMenu(5);

			if (Int32.TryParse(selectedOSBBIdInDropDown, out idSelectedOSBB) && Int32.TryParse(selectedNews, out idNew))
			{
				if (myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdOSBB == idSelectedOSBB && u.IdUser == user.Id) != null)
				{
					ViewBag.User = user;
					ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);
					ViewBag.SelectedOSBB = myOSBB.OSBBs.FirstOrDefault(u => u.Id == idSelectedOSBB);
					ViewBag.New = myOSBB.News.FirstOrDefault(n => n.IdOSBB == idSelectedOSBB && n.Id == idNew);
					//var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
					//                 from OSBB in myOSBB.OSBBs
					//                 where ((UserOSBBApartment.IdUser == user.Id) &&
					//                        (OSBB.Id == UserOSBBApartment.IdOSBB))
					//                 select new
					//                 {
					//                     osbbID = UserOSBBApartment.IdOSBB
					//                 }
					//).Distinct().ToList();

					foreach (var osbb in userOSBBs)
					{
						osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
					}
					AddNewModel addNewModel = new AddNewModel { newObj = ViewBag.New, OSBBList = osbbVal };

					ViewBag.PageItemsList = pageItemsList;
					return View(addNewModel);

				}
				else
				{
					return RedirectToAction("News", "Main",
						new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).ToString() });
				}
			}
			else
			{
				return RedirectToAction("News", "Main",
					new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).ToString() });
			}
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult EditNew(AddNewModel model, HttpPostedFileBase upload)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			model.newObj.Author = (user.LastName + " " + user.FirstName + " " + user.MiddleName).ToString();
			model.newObj.Date = DateTime.Now.Date;
			if (upload != null)
			{
				string fileName = System.IO.Path.GetFileName(upload.FileName);
				New newToUpdate = myOSBB.News.FirstOrDefault(n => n.Id == model.newObj.Id);
				string fullPath = Request.MapPath("~" + newToUpdate.PathToPhoto);
				if (System.IO.File.Exists(fullPath))
				{
					System.IO.File.Delete(fullPath);
				}
				upload.SaveAs(Server.MapPath("~/Content/images/news/" + fileName));
				model.newObj.PathToPhoto = "/Content/images/news/" + fileName;
				myOSBB.Entry(newToUpdate).CurrentValues.SetValues(model.newObj);
			}
			else
			{
				New newToUpdate = myOSBB.News.FirstOrDefault(n => n.Id == model.newObj.Id);
				myOSBB.Entry(newToUpdate).CurrentValues.SetValues(model.newObj);
			}


			myOSBB.SaveChanges();
			return RedirectToAction("News", "Main",
					new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).IdOSBB.ToString() });
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult DeletetNew(string selectedOSBBIdInDropDown, string selectedNews)
		{
			int idSelectedOSBB;
			int idNew;
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			if (Int32.TryParse(selectedOSBBIdInDropDown, out idSelectedOSBB) && Int32.TryParse(selectedNews, out idNew))
			{
				if (myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdOSBB == idSelectedOSBB && u.IdUser == user.Id) != null)
				{
					New newForDeleting = myOSBB.News.FirstOrDefault(n => n.Id == idNew && n.IdOSBB == idSelectedOSBB);
					string fullPath = Request.MapPath("~" + newForDeleting.PathToPhoto);
					if (System.IO.File.Exists(fullPath))
					{
						string s = fullPath.Substring(fullPath.Length - 14);
						if (s != "defaultNew.png")
						{
							System.IO.File.Delete(fullPath);
						}
						myOSBB.News.Remove(newForDeleting);
					}
				}
			}



			myOSBB.SaveChanges();
			return RedirectToAction("News", "Main",
					new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).IdOSBB.ToString() });
		}



		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult Ideas(string id)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;
			ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}



			CreateMenu(6);
			ViewBag.PageItemsList = pageItemsList;



			if (id == null || Int32.Parse(id) == 0)
			{
				if (osbbVal.Count() == 0)
				{
					ViewBag.SelectedOSBB = new OSBB();

					return View(osbbVal);
				}
				if (selectedOSBB != null)
				{
					ViewBag.SelectedOSBB = selectedOSBB;
					var ideas = (from Ideas in myOSBB.Ideas
								 where Ideas.IdOSBB == selectedOSBB.Id
								 orderby Ideas.Date descending
								 select new IdeaModel
								 {
									 id = Ideas.Id,
									 idOSBB = Ideas.IdOSBB,
									 title = Ideas.Title,
									 text = Ideas.Text,
									 pathToPhoto = Ideas.PathToPhoto,
									 date = Ideas.Date,
									 author = Ideas.Author,
									 isVouted = false,
									 quantityOfVotes = 0,
									 quantityOfY = 0,
									 quantityOfN = 0
								 }
					).ToList();

					ViewBag.Voters = (from UserOSBBApartment in myOSBB.UserOSBBApartments
									  where UserOSBBApartment.IdOSBB == selectedOSBB.Id
									  select UserOSBBApartment.IdUser).Distinct().Count();

					var allUserVotes = (from Votes in myOSBB.Votes
										where Votes.IdUser == user.Id
										select Votes).ToList();
					foreach (var idea in ideas)
					{
						foreach (var vote in allUserVotes)
						{
							if (idea.idOSBB == vote.IdOSBB)
							{
								if (vote.UserVote == 1)
								{
									idea.isVouted = true;
									idea.quantityOfVotes++;
									idea.quantityOfY++;
								}
								else if (vote.UserVote == 0)
								{
									idea.isVouted = true;
									idea.quantityOfVotes++;
									idea.quantityOfN++;
								}
							}
						}
					}
					//ViewBag.IsVoted = myOSBB.Votes.Select(s => s.IdOSBB == selectedOSBB.Id && s.IdUser == user.Id);


					//for (int i = 0; i < data.Count(); i++)
					//{
					//    for (int j = 0; j < osbbTableModel.Count(); j++)
					//    {

					//        if (data[i].IdOSBB == osbbTableModel[j].osbbID && osbbTableModel[j].flatsList == null)
					//        {
					//            osbbTableModel[j].flatsList = new List<int>();
					//            osbbTableModel[j].flatsList.Add(data[i].ApartmentNumber);
					//            osbbTableModel[j].quantityOfFlats++;
					//            //continue;
					//        }
					//        else if (data[i].IdOSBB == osbbTableModel[j].osbbID && osbbTableModel[j].flatsList != null)
					//        {
					//            osbbTableModel[j].flatsList.Add(data[i].ApartmentNumber);
					//            osbbTableModel[j].quantityOfFlats++;
					//        }
					//    }
					//}

					return View(osbbVal);
				}
				else
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					selectedOSBB = osbbVal[0];
					ViewBag.Ideas = (from Ideas in myOSBB.Ideas
									 where Ideas.IdOSBB == selectedOSBB.Id
									 orderby Ideas.Date descending
									 select Ideas).ToList();
					string query = "SELECT UserVote, COUNT(UserVote) FROM Votes WHERE IdOSBB = @IdOSBB GROUP BY UserVote";
					var data = myOSBB.Votes.SqlQuery(query, new SqlParameter("@IdOSBB", selectedOSBB.Id)).ToList();

					return View(osbbVal);
				}
			}
			else
			{
				int idosbb = Int32.Parse(id);
				ViewBag.SelectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idosbb);
				string query;

				if (ViewBag.SelectedOSBB == null)
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					//ViewBag.Ideas = (from Ideas in myOSBB.Ideas
					//                    where Ideas.IdOSBB == osbbVal[0].Id
					//                orderby Ideas.Date descending
					//                select Ideas).ToList();
					var ideas = (from Ideas in myOSBB.Ideas
								 where Ideas.IdOSBB == selectedOSBB.Id
								 orderby Ideas.Date descending
								 select new IdeaModel
								 {
									 id = Ideas.Id,
									 idOSBB = Ideas.IdOSBB,
									 title = Ideas.Title,
									 text = Ideas.Text,
									 pathToPhoto = Ideas.PathToPhoto,
									 date = Ideas.Date,
									 author = Ideas.Author,
									 isVouted = false,
									 quantityOfVotes = 0,
									 quantityOfY = 0,
									 quantityOfN = 0
								 }
					).ToList();
					var allUserVotes = (from Votes in myOSBB.Votes
										where Votes.IdUser == user.Id
										select Votes).ToList();
					foreach (var idea in ideas)
					{
						foreach (var vote in allUserVotes)
						{
							if (idea.idOSBB == vote.IdOSBB)
							{
								if (vote.UserVote == 1)
								{
									idea.isVouted = true;
									idea.quantityOfVotes++;
									idea.quantityOfY++;
								}
								else if (vote.UserVote == 0)
								{
									idea.isVouted = true;
									idea.quantityOfVotes++;
									idea.quantityOfN++;
								}
							}
						}
					}
					//query = "SELECT UserVote, COUNT(UserVote) FROM Votes WHERE IdOSBB = @IdOSBB GROUP BY UserVote";
					//var data = myOSBB.Votes.SqlQuery(query, new SqlParameter("@IdOSBB", osbbVal[0].Id)).ToList();
				}
				else
				{
					selectedOSBB = ViewBag.SelectedOSBB;
					int k = myOSBB.UserOSBBApartments.Count(s => s.IdOSBB == selectedOSBB.Id);
					if (selectedOSBB.quantityOfFlats == myOSBB.UserOSBBApartments.Count(s => s.IdOSBB == selectedOSBB.Id))
					{
						ViewBag.IsAllRegistred = true;
					}
					else
					{
						ViewBag.IsAllRegistred = false;
					}

					//ViewBag.Ideas = (from Ideas in myOSBB.Ideas
					//                 where Ideas.IdOSBB == selectedOSBB.Id
					//                 orderby Ideas.Date descending
					//                 select Ideas).ToList();
					var ideas = (from Ideas in myOSBB.Ideas
								 where Ideas.IdOSBB == selectedOSBB.Id
								 orderby Ideas.Date descending
								 select new IdeaModel
								 {
									 id = Ideas.Id,
									 idOSBB = Ideas.IdOSBB,
									 title = Ideas.Title,
									 text = Ideas.Text,
									 pathToPhoto = Ideas.PathToPhoto,
									 date = Ideas.Date,
									 author = Ideas.Author,
									 isVouted = false,
									 quantityOfVotes = 0,
									 quantityOfY = 0,
									 quantityOfN = 0
								 }
					).ToList();
					var allUserVotes = (from Votes in myOSBB.Votes
										where Votes.IdOSBB == selectedOSBB.Id
										select Votes).ToList();
					ViewBag.Voters = (from UserOSBBApartment in myOSBB.UserOSBBApartments
									  where UserOSBBApartment.IdOSBB == selectedOSBB.Id
									  select UserOSBBApartment.IdUser).Distinct().Count();
					foreach (var idea in ideas)
					{
						foreach (var vote in allUserVotes)
						{
							if (idea.id == vote.IdIdea && vote.IdUser == user.Id)
							{
								if (vote.UserVote == 1)
								{
									idea.isVouted = true;
									idea.quantityOfVotes++;
									idea.quantityOfY++;
								}
								else if (vote.UserVote == 0)
								{
									idea.isVouted = true;
									idea.quantityOfVotes++;
									idea.quantityOfN++;
								}
							}
                            if (idea.id == vote.IdIdea && vote.IdUser != user.Id)
                            {
                                if (vote.UserVote == 1)
                                {
                                    idea.quantityOfVotes++;
                                    idea.quantityOfY++;
                                }
                                else if (vote.UserVote == 0)
                                {
                                    idea.quantityOfVotes++;
                                    idea.quantityOfN++;
                                }
                            }
						}
					}
					ViewBag.UserVotes = (from Votes in myOSBB.Votes
										 where Votes.IdOSBB == selectedOSBB.Id
                                         select Votes).ToList();
					ViewBag.Ideas = ideas;
					//query = "SELECT UserVote, COUNT(UserVote) FROM Votes WHERE IdOSBB = @IdOSBB GROUP BY UserVote";
					//var data = myOSBB.Votes.SqlQuery(query, new SqlParameter("@IdOSBB", osbbVal[0].Id)).ToList();
				}



				return View(osbbVal);
			}
		}


		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult Idea(string selectedOSBBIdInDropDown, string selectedIdea)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;
			ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);

			CreateMenu(6);
			ViewBag.PageItemsList = pageItemsList;

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			ViewBag.UserOSBBApartments = userOSBBs;

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			int idosbb;
			int idIdea;
			Int32.TryParse(selectedIdea, out idIdea);
			Int32.TryParse(selectedOSBBIdInDropDown, out idosbb);

			if (selectedOSBBIdInDropDown == null || idosbb == 0)
			{
				if (osbbVal.Count() == 0)
				{
					ViewBag.SelectedOSBB = new OSBB();

					return View(osbbVal);
				}
				if (selectedOSBB != null)
				{
					ViewBag.SelectedOSBB = selectedOSBB;

					return View(osbbVal);
				}
				else
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					selectedOSBB = osbbVal[0];

					return View(osbbVal);
				}
			}
			else
			{
				if (myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdOSBB == idosbb && u.IdUser == user.Id) == null || myOSBB.Ideas.FirstOrDefault(i => i.Id == idIdea) == null)
				{

					return RedirectToAction("Ideas", "Main", new { id = osbbVal[0].Id });
				}
				else
				{
					ViewBag.SelectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idosbb);
					selectedOSBB = ViewBag.SelectedOSBB;
					if (selectedOSBB.quantityOfFlats == myOSBB.UserOSBBApartments.Count(s => s.IdOSBB == selectedOSBB.Id))
					{
						ViewBag.IsAllRegistred = true;
					}
					else
					{
						ViewBag.IsAllRegistred = false;
					}
					var idea = (from Ideas in myOSBB.Ideas
								where Ideas.IdOSBB == selectedOSBB.Id && Ideas.Id == idIdea
								select new IdeaModel
								{
									id = Ideas.Id,
									idOSBB = Ideas.IdOSBB,
									title = Ideas.Title,
									text = Ideas.Text,
									pathToPhoto = Ideas.PathToPhoto,
									date = Ideas.Date,
									author = Ideas.Author,
									isVouted = false,
									quantityOfVotes = 0,
									quantityOfY = 0,
									quantityOfN = 0
								}
					).ToList();
					var allUserVotes = (from Votes in myOSBB.Votes
										where Votes.IdOSBB == selectedOSBB.Id
										select Votes).ToList();

					ViewBag.Voters = (from UserOSBBApartment in myOSBB.UserOSBBApartments
									  where UserOSBBApartment.IdOSBB == selectedOSBB.Id
									  select UserOSBBApartment.IdUser).Distinct().Count();

					List<User> users = (from Users in myOSBB.Users
										from UserOSBBApartment in myOSBB.UserOSBBApartments
										where UserOSBBApartment.IdOSBB == selectedOSBB.Id && Users.Id == UserOSBBApartment.IdUser
										select Users).Distinct().ToList<User>();

					ViewBag.VoteY = new List<User>();
					ViewBag.VoteN = new List<User>();
					ViewBag.NoVote = new List<User>();
					foreach (var vote in allUserVotes)
					{
						if (idea[0].idOSBB == vote.IdOSBB && idea[0].id == vote.IdIdea)
						{
							if (vote.UserVote == 1)
							{
								idea[0].isVouted = true;
								idea[0].quantityOfVotes++;
								idea[0].quantityOfY++;
								ViewBag.VoteY.Add(users.Find(u => u.Id == vote.IdUser));
							}
							else if (vote.UserVote == 0)
							{
								idea[0].isVouted = true;
								idea[0].quantityOfVotes++;
								idea[0].quantityOfN++;
								ViewBag.VoteN.Add(users.Find(u => u.Id == vote.IdUser));
							}
							else
							{
								ViewBag.NoVote.Add(users.Find(u => u.Id == vote.IdUser));
							}
						}
					}
					foreach (var userV in users)
					{
						if (allUserVotes.Find(v => v.IdUser == userV.Id && v.IdIdea == idea[0].id) == null)
						{
							ViewBag.NoVote.Add(userV);
						}
					}


					ViewBag.Idea = idea;//myOSBB.Ideas.FirstOrDefault(i => i.IdOSBB == idosbb && i.Id == idIdea);
				}

				return View(osbbVal);
			}


		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult IdeaAdd(string selectedOSBBIdInDropDown)
		{
			int idSelectedOSBB;
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			if (Int32.TryParse(selectedOSBBIdInDropDown, out idSelectedOSBB))
			{
				if (myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdOSBB == idSelectedOSBB && u.IdUser == user.Id) != null)
				{
					ViewBag.User = user;
					ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);
					ViewBag.SelectedOSBB = myOSBB.OSBBs.FirstOrDefault(u => u.Id == idSelectedOSBB);
					var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
									 from OSBB in myOSBB.OSBBs
									 where ((UserOSBBApartment.IdUser == user.Id) &&
											(OSBB.Id == UserOSBBApartment.IdOSBB))
									 select new
									 {
										 osbbID = UserOSBBApartment.IdOSBB
									 }
					).Distinct().ToList();

					foreach (var osbb in userOSBBs)
					{
						osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
					}
                    AddIdeaModel addIdeaModel = new AddIdeaModel { idea = new Idea(), OSBBList = osbbVal };


					CreateMenu(6);
					ViewBag.PageItemsList = pageItemsList;
					return View(addIdeaModel);

				}
				else
				{
					return RedirectToAction("Ideas", "Main",
						new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).ToString() });
				}
			}
			else
			{
				return RedirectToAction("Ideas", "Main",
					new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).ToString() });
			}
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult AddIdea(AddIdeaModel model, HttpPostedFileBase upload)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			model.idea.Author = (user.LastName + " " + user.FirstName + " " + user.MiddleName).ToString();
			model.idea.Date = DateTime.Now.Date;
			if (upload != null)
			{
				string fileName = System.IO.Path.GetFileName(upload.FileName);
				upload.SaveAs(Server.MapPath("~/Content/images/ideas/" + fileName));
				model.idea.PathToPhoto = "/Content/images/ideas/" + fileName;
				myOSBB.Ideas.Add(model.idea);
			}
			else
			{
				model.idea.PathToPhoto = "/Content/images/ideas/defaultIdea.png";
				myOSBB.Ideas.Add(model.idea);
			}

			myOSBB.SaveChanges();
			EmailService emailService = new EmailService();
			var userList = (from Users in myOSBB.Users
						from UserOSBBApartments in myOSBB.UserOSBBApartments
						where Users.Id == UserOSBBApartments.IdUser && UserOSBBApartments.IdOSBB == model.idea.IdOSBB
						select Users).Distinct().ToList();
			foreach(var userV in userList)
			{
				var url = Url.Action("Ideas", "Main",
					new { id = model.idea.IdOSBB }, protocol: Request.Url.Scheme);
				emailService.SendEmail(userV.Email, "Ідея - " + model.idea.Title,
				"Більш детальніше на " + url + "\"");

				//emailService.SendEmail(userV.Email, "Голосування - " + model.idea.Title,
				//"Більш детальніше на http://kappa123kappa123-001-site1.btempurl.com/Main/Ideas/" + model.idea.IdOSBB + "/" + model.idea.Id);
			}
			
			return RedirectToAction("Ideas", "Main",
					new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).IdOSBB.ToString() });
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult DeletetIdea(string selectedOSBBIdInDropDown, string selectedIdea)
		{
			int idSelectedOSBB;
			int idIdea;
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			if (Int32.TryParse(selectedOSBBIdInDropDown, out idSelectedOSBB) && Int32.TryParse(selectedIdea, out idIdea))
			{
				if (myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdOSBB == idSelectedOSBB && u.IdUser == user.Id) != null)
				{
					Idea ideaForDeleting = myOSBB.Ideas.FirstOrDefault(i => i.Id == idIdea && i.IdOSBB == idSelectedOSBB);
					string fullPath = Request.MapPath("~" + ideaForDeleting.PathToPhoto);
					if (System.IO.File.Exists(fullPath))
					{
						string s = fullPath.Substring(fullPath.Length - 15);
						if (s != "defaultIdea.png")
						{
							System.IO.File.Delete(fullPath);
						}
						myOSBB.Ideas.Remove(ideaForDeleting);
						myOSBB.SaveChanges();
						return RedirectToAction("Ideas", "Main",
							new { id = myOSBB.UserOSBBApartments.FirstOrDefault(u => u.IdUser == user.Id).IdOSBB.ToString() });
					}
				}
			}

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			return RedirectToAction("Ideas", "Main",
							new { id = osbbVal[0].Id });
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult Vote(string selectedOSBBIdInDropDown, string idIdea, string vote)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			int idosbb = 0;
			int ididea = 0;
			int v = 0;
			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			if (Int32.TryParse(selectedOSBBIdInDropDown, out idosbb) && Int32.TryParse(idIdea, out ididea) && Int32.TryParse(vote, out v))
			{
				if (myOSBB.Ideas.FirstOrDefault(s => s.Id == ididea && s.IdOSBB == idosbb) != null)
					myOSBB.Votes.Add(new Vote
					{
						IdOSBB = idosbb,
                        IdIdea = ididea,
						IdUser = user.Id,
						UserVote = v
					});
				myOSBB.SaveChanges();
				return RedirectToAction("Ideas", "Main", new { id = selectedOSBBIdInDropDown });
			}
			else
			{
				return RedirectToAction("Idea", "Main", new { selectedOSBBIdInDropDown = osbbVal[0].Id, selectedIdea = ididea });
			}

		}

		[HttpPost]
		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult UpdateUserData(ProfileViewModel model)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			MembershipUser membershipUser = ((CustomMembershipProvider)Membership.Provider).UpdateUser(user.Id, user.Login, user.Password, model.registration.FirstName,
						model.registration.LastName, model.registration.MiddleName, model.registration.Email, model.registration.Phone, null);

			if (membershipUser != null)
			{
				return RedirectToAction("UserProfile", "Main");
			}
			else
			{
				ModelState.AddModelError("", "Не вдалось оновитидані!");
			}
			return View();
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
        public ActionResult Statistic(string id)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			ViewBag.User = user;
			ViewBag.UserDescription = myOSBB.UserDescriptions.FirstOrDefault(u => u.Id == user.Id);

			var userOSBBs = (from UserOSBBApartment in myOSBB.UserOSBBApartments
							 from OSBB in myOSBB.OSBBs
							 where ((UserOSBBApartment.IdUser == user.Id) &&
									(OSBB.Id == UserOSBBApartment.IdOSBB))
							 select new
							 {
								 osbbID = UserOSBBApartment.IdOSBB
							 }
			).Distinct().ToList();

			foreach (var osbb in userOSBBs)
			{
				osbbVal.Add(myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbb.osbbID));
			}

			CreateMenu(7);
			ViewBag.PageItemsList = pageItemsList;


			if (id == null || Int32.Parse(id) == 0)
			{
				if (osbbVal.Count() == 0)
				{
					ViewBag.SelectedOSBB = new OSBB();
					return View(osbbVal);
				}
				if (selectedOSBB != null)
				{
					ViewBag.SelectedOSBB = selectedOSBB;
					return View(osbbVal);
				}
				else
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					selectedOSBB = osbbVal[0];
					return View(osbbVal);
				}
			}
			else
			{
				int idosbb = Int32.Parse(id);
				ViewBag.SelectedOSBB = osbbVal.FirstOrDefault(o => o.Id == idosbb);
				if (ViewBag.SelectedOSBB == null)
				{
					ViewBag.SelectedOSBB = osbbVal[0];
					var userApartmants = (from PaidCommunalServices in myOSBB.PaidCommunalServices
										  where PaidCommunalServices.IdOSBB == osbbVal[0].Id && PaidCommunalServices.IdUser == user.Id
										  select PaidCommunalServices.ApartmentNumber).Distinct().ToList();

					List<List<PaidCommunalService>> paidCommunalServiceList = new List<List<PaidCommunalService>>();
					for (int i = 0; i < userApartmants.Count(); i++)
					{
						int tempApartamenNumber = userApartmants[i];
						List<PaidCommunalService> temp = (from PaidCommunalServices in myOSBB.PaidCommunalServices
														  where PaidCommunalServices.IdOSBB == osbbVal[0].Id
														  && PaidCommunalServices.ApartmentNumber == tempApartamenNumber
														   && PaidCommunalServices.IdUser == user.Id
														  select PaidCommunalServices).ToList();
						paidCommunalServiceList.Add(temp);
					}
					ViewBag.UserPaidCommunalServices = paidCommunalServiceList;

					var otherPaidCommunalServices = (from PaidCommunalServices in myOSBB.PaidCommunalServices
													 where PaidCommunalServices.IdOSBB == osbbVal[0].Id && PaidCommunalServices.IdUser != user.Id
													 select PaidCommunalServices).ToList();
					List<StatisticChartModel> statisticChartModel = new List<StatisticChartModel>();
					ViewBag.IdTypeServicesList = (from UserAccountServices in myOSBB.UserAccountServices
												  where UserAccountServices.IdUser == user.Id
												  select UserAccountServices.IdTypeServices).Distinct().ToList();
					for (int i = 0; i < 12; i++)
					{
						for (int j = 0; j < Enumerable.Count(ViewBag.IdTypeServicesList); j++)
						{
							statisticChartModel.Add(new StatisticChartModel { Month = i + 1, IdTypeServices = j + 1, AverageSumService = 0 });
						}
					}
					for (int i = 0; i < statisticChartModel.Count(); i++)
					{
						for (int j = 0; j < otherPaidCommunalServices.Count(); j++)
						{
							if (statisticChartModel[i].IdTypeServices == otherPaidCommunalServices[j].IdTypeServices && statisticChartModel[i].Month == otherPaidCommunalServices[j].Date.Month)
							{
								statisticChartModel[i].AverageSumService += otherPaidCommunalServices[j].Sum;
							}
						}
						statisticChartModel[i].AverageSumService = statisticChartModel[i].AverageSumService / osbbVal[0].quantityOfFlats;
					}
					ViewBag.AccountTypeServices = (from AccountTypeServices in myOSBB.AccountTypeServices
												   from UserAccountServices in myOSBB.UserAccountServices
												   where AccountTypeServices.Id == UserAccountServices.IdTypeServices
												   select AccountTypeServices).Distinct().ToList();
					ViewBag.StatisticChartModel = statisticChartModel;

				}
				else
				{
					selectedOSBB = ViewBag.SelectedOSBB;
					var userApartmants = (from PaidCommunalServices in myOSBB.PaidCommunalServices
									  where PaidCommunalServices.IdOSBB == selectedOSBB.Id && PaidCommunalServices.IdUser == user.Id
									  select PaidCommunalServices.ApartmentNumber).Distinct().ToList();

					List<List<PaidCommunalService>> paidCommunalServiceList = new List<List<PaidCommunalService>>();
					for (int i = 0; i < userApartmants.Count(); i++)
					{
						int tempApartamenNumber = userApartmants[i];
						List<PaidCommunalService> temp = (from PaidCommunalServices in myOSBB.PaidCommunalServices
														  where PaidCommunalServices.IdOSBB == selectedOSBB.Id
														  && PaidCommunalServices.ApartmentNumber == tempApartamenNumber
														   && PaidCommunalServices.IdUser == user.Id
														  select PaidCommunalServices).ToList();
						paidCommunalServiceList.Add(temp);
					}
					ViewBag.UserPaidCommunalServices = paidCommunalServiceList;
					//ViewBag.UserPaidCommunalServices = (from PaidCommunalServices in myOSBB.PaidCommunalServices
					//									where PaidCommunalServices.IdOSBB == selectedOSBB.Id && PaidCommunalServices.IdUser == user.Id
					//									select PaidCommunalServices).ToList();

					var otherPaidCommunalServices = (from PaidCommunalServices in myOSBB.PaidCommunalServices
													 where PaidCommunalServices.IdOSBB == selectedOSBB.Id && PaidCommunalServices.IdUser != user.Id
													 select PaidCommunalServices).ToList();
					List<StatisticChartModel> statisticChartModel = new List<StatisticChartModel>();
                    //var k = myOSBB.UserAccountServices.Distinct().Count(a => a.IdUser == user.Id);
                    ViewBag.IdTypeServicesList = (from UserAccountServices in myOSBB.UserAccountServices
							 where UserAccountServices.IdUser == user.Id
							 select UserAccountServices.IdTypeServices).Distinct().ToList();
					for (int i = 0; i < 12; i++)
					{
						for (int j = 0; j < Enumerable.Count(ViewBag.IdTypeServicesList); j++)
						{
							statisticChartModel.Add(new StatisticChartModel { Month = i + 1, IdTypeServices = j + 1, AverageSumService = 0 });
						}
					}
					//    new List<StatisticChartModel>
					//{
					//        new StatisticChartModel{ Month = 1, IdTypeServices = 1, Sum = 0 },
					//        new StatisticChartModel{ Month = 1, IdTypeServices = 2, Sum = 0 },
					//        new StatisticChartModel{ Month = 1, IdTypeServices = 3, Sum = 0 },

					//        new StatisticChartModel{ Month = 1, IdTypeServices = 1, Sum = 0 },
					//        new StatisticChartModel{ Month = 1, IdTypeServices = 2, Sum = 0 },
					//        new StatisticChartModel{ Month = 1, IdTypeServices = 3, Sum = 0 },

					//        new StatisticChartModel{ Month = 1, IdTypeServices = 1, Sum = 0 },
					//        new StatisticChartModel{ Month = 1, IdTypeServices = 2, Sum = 0 },
					//        new StatisticChartModel{ Month = 1, IdTypeServices = 3, Sum = 0 },

					//        new StatisticChartModel{ Month = 1, IdTypeServices = 1, Sum = 0 },
					//        new StatisticChartModel{ Month = 1, IdTypeServices = 2, Sum = 0 },
					//        new StatisticChartModel{ Month = 1, IdTypeServices = 3, Sum = 0 },

					//        new StatisticChartModel{ Month = 1, IdTypeServices = 1, Sum = 0 }
					// };
					//decimal gazSum = 0;
					//decimal electricitySum = 0;
					//decimal waterSum = 0;
					for (int i = 0; i < statisticChartModel.Count(); i++)
					{
						for (int j = 0; j < otherPaidCommunalServices.Count(); j++)
						{
							if (statisticChartModel[i].IdTypeServices == otherPaidCommunalServices[j].IdTypeServices && statisticChartModel[i].Month == otherPaidCommunalServices[j].Date.Month)
							{
								statisticChartModel[i].AverageSumService += otherPaidCommunalServices[j].Sum;
							}
						}
						statisticChartModel[i].AverageSumService = statisticChartModel[i].AverageSumService / selectedOSBB.quantityOfFlats;
					}
                    ViewBag.AccountTypeServices = (from AccountTypeServices in myOSBB.AccountTypeServices
                                                   from UserAccountServices in myOSBB.UserAccountServices
                                                   where AccountTypeServices.Id == UserAccountServices.IdTypeServices
                                                   select AccountTypeServices).Distinct().ToList();
                    ViewBag.StatisticChartModel = statisticChartModel;
				}

				return View(osbbVal);
			}
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public ActionResult LogOff()
		{
			FormsAuthentication.SignOut();

			return RedirectToAction("Login", "Account");
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public void CreateMenu(int numberOfActiveElement)
		{
			for (int i = 0; i < pageItemsList.Count; i++)
			{
				if (i == numberOfActiveElement)
				{
					pageItemsList[i].Active = "active";
				}
				else
				{
					pageItemsList[i].Active = "";
				}
			}
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public JsonResult getDistricts(int regionId)
		{
			var districts = (from Districts in myOSBB.Districts
							 where Districts.IdRegion == regionId
							 select Districts).ToList();
			//ViewBag.Districts = new SelectList(myOSBB.Districts, "Id", "Name");
			return Json(new SelectList(districts.ToArray(), "Id", "Name"), JsonRequestBehavior.AllowGet);
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public JsonResult getCities(int districtsId)
		{
			var districts = (from Cities in myOSBB.Cities
							 where Cities.IdDistrict == districtsId
							 select Cities).ToList();
			return Json(new SelectList(districts.ToArray(), "Id", "Name"), JsonRequestBehavior.AllowGet);
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public JsonResult getOSBBS(int regionId, int districtsId, int citiesId)
		{
			var osbbs = (from OSBBs in myOSBB.OSBBs
						 where OSBBs.IdRegion == regionId && OSBBs.IdDistrict == districtsId && OSBBs.IdCity == citiesId
						 select OSBBs).ToList();
			return Json(new SelectList(osbbs.ToArray(), "Id", "Name"), JsonRequestBehavior.AllowGet);
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public JsonResult getUserOSBBApartments(int osbbId)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			var osbbs = (from OSBBs in myOSBB.OSBBs
						 from userOSBBApartments in myOSBB.UserOSBBApartments
						 where OSBBs.Id == osbbId && OSBBs.Id == userOSBBApartments.IdOSBB
						 orderby userOSBBApartments.ApartmentNumber ascending
						 select userOSBBApartments.ApartmentNumber).ToList();
			var quantityOSBBs = myOSBB.OSBBs.FirstOrDefault(o => o.Id == osbbId).quantityOfFlats;

			var osbbsTemp = (from OSBBs in myOSBB.OSBBs
							 from AddTempOSBBs in myOSBB.AddTempOSBBs
							 where OSBBs.Id == osbbId && OSBBs.Id == AddTempOSBBs.IdOSBB
							 orderby AddTempOSBBs.ApartmentNumber ascending
							 select AddTempOSBBs.ApartmentNumber).ToList();

			List<SelectListItem> freeosbb = new List<SelectListItem>();
			if (osbbs.Count == 0)
			{
				for (int i = 1; i <= quantityOSBBs; i++)
				{
					freeosbb.Add(new SelectListItem()
					{
						Value = i.ToString(),
						Text = i.ToString()
					});
				}
			}
			else
			{
				int stepForOSBBs = 0;
				int stepForOSBBsTemp = 0;
				bool isNotFreeApartament = false;
				for (int i = 1; i <= quantityOSBBs; i++)
				{
					for (int j = stepForOSBBs; j < osbbs.Count(); j++)
					{
						if (osbbs[j] == i)
						{
							isNotFreeApartament = true;
							stepForOSBBs++;
						}
						else
						{
							continue;
						}
					}
					for (int j = stepForOSBBsTemp; j < osbbsTemp.Count(); j++)
					{
						if (osbbsTemp[j] == i)
						{
							isNotFreeApartament = true;
							stepForOSBBsTemp++;
						}
						else
						{
							continue;
						}
					}
					if (isNotFreeApartament == false)
					{
						freeosbb.Add(new SelectListItem()
						{
							Value = i.ToString(),
							Text = i.ToString()
						});

					}
					else
					{
						isNotFreeApartament = false;
						continue;
					}
				}
			}
			return Json(new SelectList(freeosbb.ToArray(), "Value", "Text"), JsonRequestBehavior.AllowGet);
		}

		[MyFilter(Roles = "ROLE_HEAD, ROLE_USER")]
		public JsonResult getQuantityOfFlats(int osbbId)
		{
			User user = myOSBB.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
			var osbbs = (from OSBBs in myOSBB.OSBBs
						 from userOSBBApartments in myOSBB.UserOSBBApartments
						 where userOSBBApartments.IdUser == user.Id && OSBBs.Id == userOSBBApartments.IdOSBB
						 select userOSBBApartments.ApartmentNumber).ToList();
			return Json(new SelectList(osbbs.ToArray(), "Id", "Quantity"), JsonRequestBehavior.AllowGet);
		}
	}
}