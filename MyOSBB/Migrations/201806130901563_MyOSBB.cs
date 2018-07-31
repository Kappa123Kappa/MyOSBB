namespace MyOSBB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MyOSBB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccountTypeServices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AddTempOSBBs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdUser = c.Int(nullable: false),
                        Email = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        MiddleName = c.String(),
                        IdOSBB = c.Int(nullable: false),
                        OSBBName = c.String(),
                        ApartmentNumber = c.Int(nullable: false),
                        Action = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdDistrict = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Conversations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdSender = c.Int(nullable: false),
                        IdReceiver = c.Int(nullable: false),
                        Message = c.String(),
                        Status = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Districts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdRegion = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Ideas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdOSBB = c.Int(nullable: false),
                        Title = c.String(),
                        Text = c.String(),
                        PathToPhoto = c.String(),
                        Date = c.DateTime(nullable: false),
                        Author = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdOSBB = c.Int(nullable: false),
                        User = c.String(),
                        MessageDate = c.DateTime(nullable: false),
                        Text = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.News",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdOSBB = c.Int(nullable: false),
                        Title = c.String(),
                        Text = c.String(),
                        PathToPhoto = c.String(),
                        Date = c.DateTime(nullable: false),
                        Author = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OSBBAccountServices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdOSBB = c.Int(nullable: false),
                        IdType = c.Int(nullable: false),
                        AccountNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OSBBs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdRegion = c.Int(nullable: false),
                        IdDistrict = c.Int(nullable: false),
                        IdCity = c.Int(nullable: false),
                        Name = c.String(),
                        IdHead = c.Int(nullable: false),
                        quantityOfFlats = c.Int(nullable: false),
                        isRegistered = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PaidCommunalServices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdUser = c.Int(nullable: false),
                        IdOSBB = c.Int(nullable: false),
                        ApartmentNumber = c.Int(nullable: false),
                        IdTypeServices = c.Int(nullable: false),
                        Sum = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Regions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserAccountServices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdUser = c.Int(nullable: false),
                        IdOSBB = c.Int(nullable: false),
                        ApartmentNumber = c.Int(nullable: false),
                        IdTypeServices = c.Int(nullable: false),
                        AccountNumber = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserDescriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Card = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserOSBBApartments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdUser = c.Int(nullable: false),
                        IdOSBB = c.Int(nullable: false),
                        ApartmentNumber = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Login = c.String(),
                        Password = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        MiddleName = c.String(),
                        Email = c.String(),
                        Phone = c.String(),
                        CodeForResetPassword = c.String(),
                        RoleId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Roles", t => t.RoleId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Votes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdOSBB = c.Int(nullable: false),
                        IdIdea = c.Int(nullable: false),
                        IdUser = c.Int(nullable: false),
                        UserVote = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "RoleId", "dbo.Roles");
            DropIndex("dbo.Users", new[] { "RoleId" });
            DropTable("dbo.Votes");
            DropTable("dbo.Users");
            DropTable("dbo.UserOSBBApartments");
            DropTable("dbo.UserDescriptions");
            DropTable("dbo.UserAccountServices");
            DropTable("dbo.Roles");
            DropTable("dbo.Regions");
            DropTable("dbo.PaidCommunalServices");
            DropTable("dbo.OSBBs");
            DropTable("dbo.OSBBAccountServices");
            DropTable("dbo.News");
            DropTable("dbo.Messages");
            DropTable("dbo.Ideas");
            DropTable("dbo.Districts");
            DropTable("dbo.Conversations");
            DropTable("dbo.Cities");
            DropTable("dbo.AddTempOSBBs");
            DropTable("dbo.AccountTypeServices");
        }
    }
}
