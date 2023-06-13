using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PterodactylPavlovServerController.Migrations.PavlovServer
{
    /// <inheritdoc />
    public partial class ModIo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Item",
                table: "WarmupLoadouts",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Gun",
                table: "WarmupLoadouts",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Attachment",
                table: "WarmupLoadouts",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NameId",
                table: "Maps",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameId",
                table: "Maps");

            migrationBuilder.UpdateData(
                table: "WarmupLoadouts",
                keyColumn: "Item",
                keyValue: null,
                column: "Item",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Item",
                table: "WarmupLoadouts",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "WarmupLoadouts",
                keyColumn: "Gun",
                keyValue: null,
                column: "Gun",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Gun",
                table: "WarmupLoadouts",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "WarmupLoadouts",
                keyColumn: "Attachment",
                keyValue: null,
                column: "Attachment",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Attachment",
                table: "WarmupLoadouts",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
