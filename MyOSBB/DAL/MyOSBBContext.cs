using MyOSBB.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MyOSBB.DAL
{
    public class MyOSBBContext : DbContext
    {
        public MyOSBBContext() : base("MyOSBBConnection")
        { }
 
        public DbSet<User> Users { get; set; }

        public DbSet<UserDescription> UserDescriptions { get; set; }
        
        public DbSet<Role> Roles { get; set; }

        public DbSet<Region> Regions { get; set; }

        public DbSet<District> Districts { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<OSBB> OSBBs { get; set; }

        public DbSet<UserOSBBApartment> UserOSBBApartments { get; set; }

        public DbSet<AddTempOSBB> AddTempOSBBs { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<AccountTypeService> AccountTypeServices { get; set; }

        public DbSet<UserAccountService> UserAccountServices { get; set; }

        public DbSet<OSBBAccountService> OSBBAccountServices { get; set; }

        public DbSet<New> News { get; set; }

        public DbSet<Idea> Ideas { get; set; }

        public DbSet<Vote> Votes { get; set; }

		public DbSet<PaidCommunalService> PaidCommunalServices { get; set; }
	}
}