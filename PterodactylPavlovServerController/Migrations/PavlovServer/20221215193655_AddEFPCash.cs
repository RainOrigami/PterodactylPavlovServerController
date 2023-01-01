using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PterodactylPavlovServerController.Migrations.PavlovServer
{
    /// <inheritdoc />
    public partial class AddEFPCash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EFPCash",
                table: "Players",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EFPCash",
                table: "Players");
        }
    }
}
