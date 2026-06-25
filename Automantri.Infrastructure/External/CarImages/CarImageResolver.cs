using Automantri.Application.Cars;
using Automantri.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Automantri.Infrastructure.External.CarImages;

internal sealed class CarImageResolver(IOptions<CarImagesOptions> options) : ICarImageResolver
{
    public string ResolveImageUrl(string make, string model, int year)
    {
        var template = options.Value.UrlTemplate;

        return template
            .Replace("{make}", Uri.EscapeDataString(make), StringComparison.OrdinalIgnoreCase)
            .Replace("{model}", Uri.EscapeDataString(model), StringComparison.OrdinalIgnoreCase)
            .Replace("{year}", year.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
