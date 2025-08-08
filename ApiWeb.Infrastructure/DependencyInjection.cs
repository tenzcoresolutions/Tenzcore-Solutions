using ApiWeb.Application.Interfaces;
using ApiWeb.Infrastructure.Persistence;
using ApiWeb.Infrastructure.Security;
using ApiWeb.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiWeb.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                               ?? "Server=localhost,1433;Database=ApiWebDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.Configure<IdentityHashingOptions>(configuration.GetSection("IdentityHashing"));
        services.AddScoped<IClientIdentityHasher, ClientIdentityHasher>();
        services.AddScoped<IMessageService, MessageService>();

        return services;
    }
}