using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automantri.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogFieldsAndVariantKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cars_make_model",
                table: "cars");

            migrationBuilder.AlterColumn<string>(
                name: "source_query",
                table: "cars",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AddColumn<string>(
                name: "car_type",
                table: "cars",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "end_production_year",
                table: "cars",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "generation",
                table: "cars",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "serie",
                table: "cars",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "specifications_json",
                table: "cars",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "start_production_year",
                table: "cars",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trim",
                table: "cars",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(
                """
                DELETE FROM cars
                WHERE id IN (
                    SELECT id
                    FROM (
                        SELECT id,
                               ROW_NUMBER() OVER (
                                   PARTITION BY lower(make), lower(model), year, coalesce(transmission, ''), coalesce(trim, '')
                                   ORDER BY updated_at_utc DESC, retrieved_at_utc DESC
                               ) AS row_number
                        FROM cars
                    ) duplicates
                    WHERE duplicates.row_number > 1
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_cars_make_model_year_transmission_trim",
                table: "cars",
                columns: new[] { "make", "model", "year", "transmission", "trim" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cars_make_model_year_transmission_trim",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "car_type",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "end_production_year",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "generation",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "serie",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "specifications_json",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "start_production_year",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "trim",
                table: "cars");

            migrationBuilder.AlterColumn<string>(
                name: "source_query",
                table: "cars",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.CreateIndex(
                name: "IX_cars_make_model",
                table: "cars",
                columns: new[] { "make", "model" },
                unique: true);
        }
    }
}
