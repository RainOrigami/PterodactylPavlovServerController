using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PterodactylPavlovServerController.Migrations.PavlovServer
{
    /// <inheritdoc />
    public partial class WarmupLoadouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarmupItems");

            migrationBuilder.CreateTable(
                name: "WarmupLoadouts",
                columns: table => new
                {
                    ServerId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gun = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Item = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Attachment = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarmupLoadouts", x => new { x.ServerId, x.Name });
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarmupLoadouts");

            migrationBuilder.CreateTable(
                name: "WarmupItems",
                columns: table => new
                {
                    ServerId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Item = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarmupItems", x => new { x.ServerId, x.Item });
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
