using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IA.Identity.API.Migrations
{
    public partial class jwks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityKeys");

            migrationBuilder.CreateTable(
                name: "NetDevPack.Security.Jwt.Store.EntityFrameworkCore.ISecurityKeyContext.SecurityKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeyId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetDevPack.Security.Jwt.Store.EntityFrameworkCore.ISecurityKeyContext.SecurityKeys", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NetDevPack.Security.Jwt.Store.EntityFrameworkCore.ISecurityKeyContext.SecurityKeys");

            migrationBuilder.CreateTable(
                name: "SecurityKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    JweAlgorithm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JweEncryption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JwkType = table.Column<int>(type: "int", nullable: false),
                    JwsAlgorithm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KeyId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityKeys", x => x.Id);
                });
        }
    }
}
