namespace FluentMigrator.Tests.Integration.Migrations.InvalidForTestMigrations
{
    [Migration(2)]
    public class AddPermissionToRoles : Migration
    {
        public override void Up()
        {
            Create.Table("Role")
                .WithColumn("Id")
                .AsInt32()
                .Identity();
            
            Create.Column("Permissions")
                .OnTable("Roles")
                .AsInt32()
                .Nullable();
        }

        public override void Down()
        {
            Delete.Column("Permissions")
                .FromTable("User");
        }
    }
}