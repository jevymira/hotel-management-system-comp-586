using Application.Abstractions.Services;

namespace Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAdminAccountService _accountService;
    private readonly IJWTService _jwtService;
    public AuthenticationService(IAdminAccountService accountService, IJWTService jwtService)
    {
        _accountService = accountService;
        _jwtService = jwtService;
    }
    public async Task<string?> Login(string email, string passwordHash)
    {
        string? id = await _accountService.GetIDIfActiveValidCredentials(email, passwordHash);
        if (id == null)
            return null;
        return _jwtService.IssueToken(id);
    }
}
