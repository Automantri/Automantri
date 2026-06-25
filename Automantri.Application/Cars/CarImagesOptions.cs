namespace Automantri.Application.Cars;

public sealed class CarImagesOptions
{
    public const string SectionName = "CarImages";

    /// <summary>
    /// URL template with placeholders: {make}, {model}, {year}.
    /// API Ninjas does not provide images; this resolves a display URL for the UI.
    /// </summary>
    public string UrlTemplate { get; set; } =
        "https://cdn.imagin.studio/getImage?customer=img&make={make}&modelFamily={model}&year={year}&angle=23&width=800";
}
