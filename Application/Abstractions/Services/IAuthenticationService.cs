namespace Application.Abstractions.Services;

public interface IAuthenticationService
{
    public Task<string?> Login(string email, string passwordHash);
}
