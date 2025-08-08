using System.Security.Cryptography;
using System.Text;
using ApiWeb.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ApiWeb.Infrastructure.Security;

public class ClientIdentityHasher : IClientIdentityHasher
{
    private readonly IdentityHashingOptions _options;

    public ClientIdentityHasher(IOptions<IdentityHashingOptions> options)
    {
        _options = options.Value;
    }

    public string ComputeClientHash(string? ipAddress, string? userAgent)
    {
        var input = $"{ipAddress ?? "unknown"}|{userAgent ?? "unknown"}|{_options.Salt}";
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}