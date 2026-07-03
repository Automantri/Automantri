namespace Automantri.Application.Cars;

public sealed record CarSearchResultDto(
    IReadOnlyCollection<Frontend.CarListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
