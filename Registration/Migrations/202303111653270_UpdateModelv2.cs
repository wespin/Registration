namespace Registration.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateModelv2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Teacher",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        LastName = c.String(nullable: false, maxLength: 50),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        HireDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.CourseTeacher",
                c => new
                    {
                        CourseID = c.Int(nullable: false),
                        TeacherID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CourseID, t.TeacherID })
                .ForeignKey("dbo.Course", t => t.CourseID, cascadeDelete: true)
                .ForeignKey("dbo.Teacher", t => t.TeacherID, cascadeDelete: true)
                .Index(t => t.CourseID)
                .Index(t => t.TeacherID);
            
            AlterColumn("dbo.Course", "Title", c => c.String(maxLength: 50));
            AlterColumn("dbo.Student", "LastName", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CourseTeacher", "TeacherID", "dbo.Teacher");
            DropForeignKey("dbo.CourseTeacher", "CourseID", "dbo.Course");
            DropIndex("dbo.CourseTeacher", new[] { "TeacherID" });
            DropIndex("dbo.CourseTeacher", new[] { "CourseID" });
            AlterColumn("dbo.Student", "LastName", c => c.String());
            AlterColumn("dbo.Course", "Title", c => c.String());
            DropTable("dbo.CourseTeacher");
            DropTable("dbo.Teacher");
        }
    }
}
