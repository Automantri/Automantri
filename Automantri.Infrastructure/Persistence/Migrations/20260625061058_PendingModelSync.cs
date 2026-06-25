using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automantri.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_cars",
                table: "cars");

            migrationBuilder.RenameIndex(
                name: "ix_cars_make_model_year_transmission",
                table: "cars",
                newName: "IX_cars_make_model_year_transmission");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cars",
                table: "cars",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_cars",
                table: "cars");

            migrationBuilder.RenameIndex(
                name: "IX_cars_make_model_year_transmission",
                table: "cars",
                newName: "ix_cars_make_model_year_transmission");

            migrationBuilder.AddPrimaryKey(
                name: "pk_cars",
                table: "cars",
                column: "id");
        }
    }
}
