namespace Automantri.Application.Cars;

public sealed record CarSearchQuery(
    string? Search = null,
    string? Make = null,
    string? Model = null,
    string? FuelType = null,
    string? VehicleClass = null,
    string? Transmission = null,
    int? YearFrom = null,
    int? YearTo = null,
    int Page = 1,
    int PageSize = 20)
{
    public int SafePage => Page < 1 ? 1 : Page;

    public int SafePageSize => PageSize switch
    {
        < 1 => 20,
        > 100 => 100,
        _ => PageSize
    };
}
