using Microsoft.EntityFrameworkCore.Migrations;

namespace IA.Identity.API.Migrations
{
    public partial class AddITIN : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ITIN",
                table: "AspNetUsers",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ITIN",
                table: "AspNetUsers");
        }
    }
}
