using Automantri.Domain.Entities;

namespace Automantri.Application.Cars;

public static class CarIdentity
{
    public static string BuildKey(Car car)
    {
        return string.Join(
            '|',
            Normalize(car.Make),
            Normalize(car.Model),
            car.Year.ToString(),
            Normalize(car.Transmission),
            Normalize(car.Trim));
    }

    public static void NormalizeCar(Car car)
    {
        car.Make = Normalize(car.Make);
        car.Model = Normalize(car.Model);
        car.Transmission = string.IsNullOrWhiteSpace(car.Transmission) ? null : car.Transmission.Trim();
        car.Trim = string.IsNullOrWhiteSpace(car.Trim) ? null : car.Trim.Trim();
    }

    public static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
}
