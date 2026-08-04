namespace Automantri.Application.Auth;

public sealed class AdminAuthOptions
{
    public const string SectionName = "AdminAuth";

    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "Admin@123";
    public string JwtKey { get; set; } = "Automantri-Dev-Secret-Key-Change-In-Production-2026!";
    public string Issuer { get; set; } = "Automantri";
    public string Audience { get; set; } = "AutomantriFrontend";
    public int TokenLifetimeHours { get; set; } = 12;
}
