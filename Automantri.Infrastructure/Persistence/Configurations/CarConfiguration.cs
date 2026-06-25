using Automantri.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automantri.Infrastructure.Persistence.Configurations;

internal sealed class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.ToTable("cars");

        builder.HasKey(car => car.Id);

        builder.Property(car => car.Id).HasColumnName("id");
        builder.Property(car => car.CityMpg).HasColumnName("city_mpg");
        builder.Property(car => car.VehicleClass).HasColumnName("vehicle_class").HasMaxLength(120);
        builder.Property(car => car.CombinationMpg).HasColumnName("combination_mpg");
        builder.Property(car => car.Cylinders).HasColumnName("cylinders");
        builder.Property(car => car.Displacement).HasColumnName("displacement").HasPrecision(8, 2);
        builder.Property(car => car.Drive).HasColumnName("drive").HasMaxLength(50);
        builder.Property(car => car.FuelType).HasColumnName("fuel_type").HasMaxLength(50);
        builder.Property(car => car.HighwayMpg).HasColumnName("highway_mpg");
        builder.Property(car => car.Make).HasColumnName("make").HasMaxLength(120);
        builder.Property(car => car.Model).HasColumnName("model").HasMaxLength(120);
        builder.Property(car => car.Transmission).HasColumnName("transmission").HasMaxLength(20);
        builder.Property(car => car.Year).HasColumnName("year");
        builder.Property(car => car.Trim).HasColumnName("trim").HasMaxLength(200);
        builder.Property(car => car.Generation).HasColumnName("generation").HasMaxLength(200);
        builder.Property(car => car.Serie).HasColumnName("serie").HasMaxLength(200);
        builder.Property(car => car.CarType).HasColumnName("car_type").HasMaxLength(50);
        builder.Property(car => car.StartProductionYear).HasColumnName("start_production_year");
        builder.Property(car => car.EndProductionYear).HasColumnName("end_production_year");
        builder.Property(car => car.SpecificationsJson).HasColumnName("specifications_json").HasColumnType("jsonb");
        builder.Property(car => car.SourceQuery).HasColumnName("source_query").HasMaxLength(250);
        builder.Property(car => car.ImageUrl).HasColumnName("image_url").HasMaxLength(2000);
        builder.Property(car => car.RetrievedAtUtc).HasColumnName("retrieved_at_utc");
        builder.Property(car => car.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(car => new { car.Make, car.Model, car.Year, car.Transmission, car.Trim })
            .IsUnique();
    }
}
