using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Abstractions.Services;
using Application.Models;

namespace API.Controllers;

[Authorize]
[Route("api/admin-accounts")]
public class AdminAccountsController : ControllerBase
{
    private IAuthenticationService _authenticationService;
    private IAdminAccountService _adminAccountService;

    public AdminAccountsController(
        IAuthenticationService authenticationService,
        IAdminAccountService adminAccountService)
    {
        _authenticationService = authenticationService;
        _adminAccountService = adminAccountService;
    }

    // use case: Login page
    // contains AdminID in "sub" of token returned, see at jwt.io Debugger
    // SHA-256 for password
    /* sample request body:
    {
        "email": "apierce@travelersinn.com",
        "password": "{SHA-256 hash}" // see AdminAccounts table in DynamoDB us-east-1
    }
    */
    [AllowAnonymous]
    [HttpPost("login")] // POST /api/admin-accounts/login
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // incorrect credentials
    [ProducesResponseType(StatusCodes.Status200OK)] // with token in response body
    public async Task<IActionResult> PostAsync([FromBody] LoginRequest loginRequest)
    {
        string? token = await _authenticationService.Login(loginRequest.Email, loginRequest.Password);
        if (token == null)
            return Unauthorized("Credentials invalid or account inactive.");
        else 
            return Ok(token);
    }

    [HttpGet("{id}")] // GET /api/admin-accounts/{id}
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ActionName(nameof(GetAsync))] // CreatedAtAction and .NET Async suffixing
    public async Task<IActionResult> GetAsync(string id)
    {
        var account = await _adminAccountService.GetAsync(id);
        if (account == null) { return NotFound($"No account exists with ID {id}."); }
        return Ok(account);
    }

    // use case: Admin Accounts page, Accounts
    [HttpGet] // GET /api/admin-accounts
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        return Ok(await _adminAccountService.GetAllAsync());
    }

    // use case: Admin Accounts page, Create Account 
    /* sample request body:
    {
        "fullName": "Aiden Pierce",
        "email": "apierce@travelersinn.com",
        "passwordHash": "{SHA-256}"
    }
    */
    [HttpPost] // POST /api/admin-accounts
    [ProducesResponseType(StatusCodes.Status409Conflict)] // when email already in use
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> PostAsync([FromBody] CreateAccountDTO dto)
    {
        try
        {
            var account = await _adminAccountService.AddAsync(dto);
            return CreatedAtAction(nameof(GetAsync), new { id = account.AdminID }, value: account);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // use case: Admin Accounts page, Edit Account 
    // sample resource: /api/admin-accounts/0123456789
    /* sample request body
    {
        "fullName": "Aiden Pierce",
        "email": "apierce@travelersinn.com",
        "accountStatus": "Active",
        "updatedBy": "TEST#ACC"
    }
    */
    [HttpPatch("{id}")] // PATCH /api/admin-accounts/{id}
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // when email already in use in another account
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchAsync([FromRoute] string id, [FromBody] UpdateAdminAccountDTO dto)
    {
        try
        {
            await _adminAccountService.UpdateDetailsAsync(id, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }

        return NoContent();
    }

    // use case: Change Password page
    /* sample request body:
    {
        "email": "apierce@travelersinn.com",
        "oldPasswordHash": "{SHA-256}", // see DynamoDB table on us-east-1
        "newPasswordHash": "{SHA-256}"
    }
    */
    [AllowAnonymous]
    [HttpPatch("reset")] // PATCH api/admin-accounts/reset
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)] // not 401, no auth header
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchPasswordAsync([FromBody] UpdatePasswordDTO dto)
    {
        if (!(await _adminAccountService.UpdatePasswordAsync(dto)))
            return UnprocessableEntity("Email or password invalid.");
        return NoContent();
    }

    // TODO: AccountStatus entity
}
