using Application.Abstractions.Services;

namespace Application.Services;

/// <summary>
/// High-level interface for application authentication.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IAdminAccountService _accountService;
    private readonly IJWTService _jwtService;
    public AuthenticationService(IAdminAccountService accountService, IJWTService jwtService)
    {
        _accountService = accountService;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Supplies the authentication token, provided a valid
    /// email/password combination.
    /// </summary>
    /// <param name="email">Admin account email</param>
    /// <param name="passwordHash">SHA-256 UPPER</param>
    public async Task<string?> Login(string email, string passwordHash)
    {
        string? id = await _accountService.GetIDIfActiveValidCredentials(email, passwordHash);
        if (id == null)
            return null;
        return _jwtService.IssueToken(id);
    }
}
