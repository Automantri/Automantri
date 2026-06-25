using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automantri.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCarImageAndUniqueMakeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cars_make_model_year_transmission",
                table: "cars");

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "cars",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at_utc",
                table: "cars",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.Sql(
                """
                DELETE FROM cars
                WHERE id IN (
                    SELECT id
                    FROM (
                        SELECT id,
                               ROW_NUMBER() OVER (
                                   PARTITION BY lower(make), lower(model)
                                   ORDER BY year DESC, retrieved_at_utc DESC
                               ) AS row_number
                        FROM cars
                    ) duplicates
                    WHERE duplicates.row_number > 1
                );
                """);

            migrationBuilder.Sql(
                """
                UPDATE cars
                SET updated_at_utc = retrieved_at_utc
                WHERE updated_at_utc = '-infinity'::timestamptz OR updated_at_utc = '0001-01-01 00:00:00+00'::timestamptz;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_cars_make_model",
                table: "cars",
                columns: new[] { "make", "model" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cars_make_model",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "updated_at_utc",
                table: "cars");

            migrationBuilder.CreateIndex(
                name: "IX_cars_make_model_year_transmission",
                table: "cars",
                columns: new[] { "make", "model", "year", "transmission" });
        }
    }
}
