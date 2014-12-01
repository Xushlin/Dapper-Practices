using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace DapperSimple.Data.Migrations
{
    [Migration(2014112901)]
    public class _2014112901_Init : Migration
    {
        public override void Up()
        {
            Create.Table("Category").WithColumn("Id").AsInt16().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(128).NotNullable()
                .WithColumn("ParentId").AsInt16().NotNullable().WithDefaultValue(0);

            Create.Table("Product").WithColumn("Id").AsInt16().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(256).NotNullable()
                .WithColumn("Price").AsInt16()
                .WithColumn("CateGoryId").AsInt16().NotNullable().ForeignKey("Category", "Id");
        }
        public override void Down()
        {
            Delete.Table("Product");
            Delete.Table("Category");
        }
    }
}
