namespace Registration.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateColumName : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Student", name: "FirstMidName", newName: "FirstName");
            AlterColumn("dbo.Student", "FirstName", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Student", "FirstName", c => c.String());
            RenameColumn(table: "dbo.Student", name: "FirstName", newName: "FirstMidName");
        }
    }
}
