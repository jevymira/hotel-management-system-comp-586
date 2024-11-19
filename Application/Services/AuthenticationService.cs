using Application.Abstractions.Services;
using Domain.Abstractions.Services;

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
        if (!(await _accountService.CheckIfActiveValidCredentials(email, passwordHash)))
            return null;
        return _jwtService.IssueToken();
    }
}
