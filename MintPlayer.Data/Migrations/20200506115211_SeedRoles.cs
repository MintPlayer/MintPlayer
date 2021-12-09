using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MintPlayer.Data.Migrations
{
    public partial class SeedRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("93c9bda5-8254-486f-ade1-95b5b66e83db"), null, "Blogger", "Blogger" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { new Guid("91f3cec8-a67d-45f3-b718-22cf71961b05"), null, "Administrator", "Administrator" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("91f3cec8-a67d-45f3-b718-22cf71961b05"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("93c9bda5-8254-486f-ade1-95b5b66e83db"));
        }
    }
}
