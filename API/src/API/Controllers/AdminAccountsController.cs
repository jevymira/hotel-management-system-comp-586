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

    // use case: Login page (SHA-256 for password)
    // contains AdminID in "sub" of token returned, see at jwt.io Debugger
    /// <summary>
    /// Log in to an existing admin account.
    /// </summary>
    /// <response code="401">Incorrect credentials.</response>
    /// <response code="200">Authenticated; token returned with AdminID in "sub".</response>
    /* sample request body:
    {
        "email": "apierce@travelersinn.com",
        "password": "{SHA-256 hash}" // see AdminAccounts table in DynamoDB us-east-1
    }
    */
    [AllowAnonymous]
    [HttpPost("login")] // POST /api/admin-accounts/login
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PostAsync([FromBody] LoginRequest loginRequest)
    {
        string? token = await _authenticationService.Login(loginRequest.Email, loginRequest.Password);
        if (token == null)
            return Unauthorized("Credentials invalid or account inactive.");
        else 
            return Ok(token);
    }

    /// <summary>
    /// Retrieve a single admin account by its unique id.
    /// </summary>
    /// <param name="id">Admin account id.</param>
    /// <response code="404">No account exists with the supplied ID.</response>
    /// <response code="200">The admin account (without its password) is retrieved successfully.</response>
    /// <example>
    /// GET {base-url}/api/admin-accounts/3dkwsQ
    /// </example>
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
    /// <summary>
    /// Retrieve all admin accounts.
    /// </summary>
    /// <response code="200">Admin accounts are retrieved successfully.</response>
    [HttpGet] // GET /api/admin-accounts
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        return Ok(await _adminAccountService.GetAllAsync());
    }

    // use case: Admin Accounts page, Create Account 
    /// <summary>
    /// Create and store a new admin account.
    /// </summary>
    /// <param name="dto">FullName, Email, and PasswordHash.</param>
    /// <response code="409">Email already in use.</response>
    /// <response code="201">Admin account created and stored successfully.</response>
    /* sample request body:
    {
        "name": "Aiden Pierce",
        "email": "apierce@travelersinn.com",
        "passwordHash": "{SHA-256}"
    }
    */
    [HttpPost] // POST /api/admin-accounts
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
    /// <summary>
    /// Edit the personal information and/or status of the existing admin account with the supplied ID.
    /// </summary>
    /// <param name="id">Admin account ID.</param>
    /// <param name="dto">FullName, Email, AccountStatus, and account UpdatedBy.</param>
    /// <response code="404">Supplied ID matches no existing accounts.</response>
    /// <response code="409">Email already in use.</response>
    /// <response code="204">Edit successful and saved.</response>
    /// <example>
    /// PATCH {base-url}/api/admin-accounts/3dkwsQ
    /// </example>
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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
    /// <summary>
    /// Change the password for the admin account with the specified email.
    /// </summary>
    /// <param name="dto">Email, OldPasswordHash, NewPasswordHash</param>
    /// <response code="422">Credentials invalid.</response>
    /// <response code="204">Password change is successful and saved.</response>
    /* sample request body:
    {
        "email": "apierce@travelersinn.com",
        "oldPasswordHash": "{SHA-256}", // see DynamoDB table on us-east-1
        "newPasswordHash": "{SHA-256}"
    }
    */
    [AllowAnonymous]
    [HttpPatch("reset")] // PATCH api/admin-accounts/reset
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchPasswordAsync([FromBody] UpdatePasswordDTO dto)
    {
        if (!(await _adminAccountService.UpdatePasswordAsync(dto)))
            return UnprocessableEntity("Email or password invalid.");
        return NoContent();
    }

}
