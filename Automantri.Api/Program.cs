using System.Net;
using System.Net.Sockets;
using System.Text;
using Automantri.Application.Auth;
using Automantri.Application.Imports;
using Automantri.Infrastructure;
using Automantri.Infrastructure.Auth;
using Automantri.Infrastructure.Imports;
using Automantri.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);

var adminAuth = builder.Configuration.GetSection(AdminAuthOptions.SectionName).Get<AdminAuthOptions>()
    ?? new AdminAuthOptions();
builder.Services.Configure<AdminAuthOptions>(builder.Configuration.GetSection(AdminAuthOptions.SectionName));
builder.Services.AddSingleton<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<ICatalogImportService, CatalogImportService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = adminAuth.Issuer,
            ValidAudience = adminAuth.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(adminAuth.JwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ??
    [
        "http://localhost:5173",
        "http://localhost:5174",
        "http://localhost:5175",
        "http://localhost:4173",
        "http://127.0.0.1:5173",
        "http://127.0.0.1:5174",
        "http://127.0.0.1:5175",
        "http://0.0.0.0:5173",
        "http://0.0.0.0:5174",
        "http://0.0.0.0:5175",
    ];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        // Vite --host 0.0.0.0 / LAN IP access sends Origin like http://0.0.0.0:5173 or http://192.168.x.x:5173
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(IsLocalDevOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

static bool IsLocalDevOrigin(string? origin)
{
    if (string.IsNullOrWhiteSpace(origin) || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
        return false;
    }

    if (uri.Scheme is not ("http" or "https"))
    {
        return false;
    }

    var host = uri.Host;
    if (host is "localhost" or "127.0.0.1" or "0.0.0.0" or "::1" or "[::1]")
    {
        return true;
    }

    if (!IPAddress.TryParse(host, out var ip))
    {
        // Allow machine hostname on the local network during development (e.g. http://my-pc:5173)
        return !host.Contains('.', StringComparison.Ordinal) || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase);
    }

    if (ip.AddressFamily != AddressFamily.InterNetwork)
    {
        return ip.IsIPv6LinkLocal || IPAddress.IsLoopback(ip);
    }

    var bytes = ip.GetAddressBytes();
    return bytes[0] == 10
           || (bytes[0] == 192 && bytes[1] == 168)
           || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31);
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AutomantriDbContext>();
    dbContext.Database.Migrate();

    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
