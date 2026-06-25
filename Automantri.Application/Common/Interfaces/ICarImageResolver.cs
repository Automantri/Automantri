namespace Automantri.Application.Common.Interfaces;

public interface ICarImageResolver
{
    string ResolveImageUrl(string make, string model, int year);
}
