namespace Automantri.Application.Auth;

public sealed record LoginRequestDto(string Username, string Password);

public sealed record LoginResultDto(
    string Token,
    string Username,
    string Role,
    DateTimeOffset ExpiresAtUtc);
