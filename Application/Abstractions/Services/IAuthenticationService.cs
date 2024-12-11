namespace Application.Abstractions.Services;

/// <summary>
/// High-level interface for application authentication.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Supplies the authentication token, provided a valid
    /// email/password combination.
    /// </summary>
    /// <param name="email">Admin account email</param>
    /// <param name="passwordHash">SHA-256 UPPER</param>
    public Task<string?> Login(string email, string passwordHash);
}
