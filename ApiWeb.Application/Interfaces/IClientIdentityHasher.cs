namespace ApiWeb.Application.Interfaces;

public interface IClientIdentityHasher
{
    string ComputeClientHash(string? ipAddress, string? userAgent);
}