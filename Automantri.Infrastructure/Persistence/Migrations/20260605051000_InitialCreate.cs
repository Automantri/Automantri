using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Automantri.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "cars",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                city_mpg = table.Column<int>(type: "integer", nullable: false),
                vehicle_class = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                combination_mpg = table.Column<int>(type: "integer", nullable: false),
                cylinders = table.Column<int>(type: "integer", nullable: true),
                displacement = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                drive = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                fuel_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                highway_mpg = table.Column<int>(type: "integer", nullable: false),
                make = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                transmission = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                year = table.Column<int>(type: "integer", nullable: false),
                source_query = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                retrieved_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_cars", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_cars_make_model_year_transmission",
            table: "cars",
            columns: ["make", "model", "year", "transmission"]);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "cars");
    }
}
