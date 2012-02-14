namespace FluentMigrator.Tests.Integration.Migrations.InvalidForTestMigrations
{
    [Migration(1)]
    public class CreateUsers : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString();
        }

        public override void Down()
        {
            Delete.Table("Users");
        }
    }
}