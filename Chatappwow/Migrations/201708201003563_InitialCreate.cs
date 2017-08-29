namespace Chatappwow.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Connections",
                c => new
                    {
                        ConnectionId = c.String(nullable: false, maxLength: 128),
                        User_UserName = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ConnectionId)
                .ForeignKey("dbo.Users", t => t.User_UserName)
                .Index(t => t.User_UserName);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserName = c.String(nullable: false, maxLength: 128),
                        Token = c.String(),
                        Room_Name = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.UserName)
                .ForeignKey("dbo.Rooms", t => t.Room_Name)
                .Index(t => t.Room_Name);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Name);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "Room_Name", "dbo.Rooms");
            DropForeignKey("dbo.Connections", "User_UserName", "dbo.Users");
            DropIndex("dbo.Users", new[] { "Room_Name" });
            DropIndex("dbo.Connections", new[] { "User_UserName" });
            DropTable("dbo.Rooms");
            DropTable("dbo.Users");
            DropTable("dbo.Connections");
        }
    }
}
