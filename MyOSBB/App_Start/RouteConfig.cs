using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MyOSBB
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			

			routes.MapRoute(
			   name: "ResetPassword",
			   url: "Account/ResetPassword/{user}/{password}",
			   defaults: new { controller = "Account", action = "ResetPassword", userId = UrlParameter.Optional, password = UrlParameter.Optional }
			);

			routes.MapRoute(
               name: "Profile",
               url: "Main/Profile/{id}",
               defaults: new { controller = "Main", action = "UserProfile", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "MyOSBB",
               url: "Main/MyOSBB/{id}",
               defaults: new { controller = "Main", action = "MyOSBB", id = UrlParameter.Optional}
            );

            routes.MapRoute(
               name: "Chat",
               url: "Main/Chat/{id}",
               defaults: new { controller = "Main", action = "Chat", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "Chat2",
               url: "Main/Chat2/{id}",
               defaults: new { controller = "Main", action = "Chat2", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "News",
               url: "Main/News/{id}",
               defaults: new { controller = "Main", action = "News", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "Ideas",
               url: "Main/Ideas/{id}",
               defaults: new { controller = "Main", action = "Ideas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "Statistic",
               url: "Main/Statistic/{id}",
               defaults: new { controller = "Main", action = "Statistic", id = UrlParameter.Optional }
            );



            routes.MapRoute(
                name: "Payment",
                url: "Main/MyOSBB/{selectedOSBBIdInDropDown}/{apartmentNumber}",
                defaults: new { controller = "Main", action = "Payment", selectedOSBBIdInDropDown = UrlParameter.Optional, apartmentNumber = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "NewAdd",
               url: "Main/News/{selectedOSBBIdInDropDown}/CreateNew",
               defaults: new { controller = "Main", action = "NewAdd", selectedOSBBIdInDropDown = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "NewEdit",
               url: "Main/News/{selectedOSBBIdInDropDown}/EditNew",
               defaults: new { controller = "Main", action = "NewEdit", selectedOSBBIdInDropDown = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "New",
                url: "Main/News/{selectedOSBBIdInDropDown}/{selectedNews}",
                defaults: new { controller = "Main", action = "New", selectedOSBBIdInDropDown = UrlParameter.Optional, selectedNews = UrlParameter.Optional }
            );

            routes.MapRoute(
              name: "IdeaAdd",
              url: "Main/Idea/{selectedOSBBIdInDropDown}/CreateIdea",
              defaults: new { controller = "Main", action = "IdeaAdd", selectedOSBBIdInDropDown = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "Idea",
                url: "Main/Idea/{selectedOSBBIdInDropDown}/{selectedIdea}",
                defaults: new { controller = "Main", action = "Idea", selectedOSBBIdInDropDown = UrlParameter.Optional, selectedIdea = UrlParameter.Optional }
            );

           
            routes.MapRoute(
                name: "SendMessage",
                url: "send_message",
                defaults: new { controller = "Main", action = "SendMessage" }
            );

            routes.MapRoute(
                name: "PusherAuth",
                url: "pusher/auth",
                defaults: new { controller = "Main", action = "AuthForChannel" }
            );

            routes.MapRoute(
                name: "ConversationWithContact",
                url: "contact/conversations/{id}",
                defaults: new { controller = "Main", action = "AuthForChannel", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
