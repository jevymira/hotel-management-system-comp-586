using Application.Models;
using Domain.Models;

namespace Application.Abstractions.Services;

/// <summary>
/// Application-layer service that provides a high-level interface for
/// admin account operations.
/// </summary>
public interface IAdminAccountService
{
    public Task<GetAdminAccountDTO> AddAsync(CreateAccountDTO accountDTO);
    public Task<GetAdminAccountDTO> GetAsync(string id);
    public Task<List<GetAdminAccountDTO>> GetAllAsync();

    /// <summary>
    /// Retrieve the admin account ID corresponding to the email/password combination.
    /// </summary>
    /// <param name="email">Admin account email</param>
    /// <param name="passwordHash">SHA-256 UPPER hash</param>
    public Task<string?> GetIDIfActiveValidCredentials(string email, string passwordHash);
    public Task UpdateDetailsAsync(string id, UpdateAdminAccountDTO updateDTO);

    /// <summary>
    /// Update admin account password, provided valid credentials.
    /// </summary>
    /// <param name="credentialsDTO">admin email, old password, new password</param>
    public Task<bool> UpdatePasswordAsync(UpdatePasswordDTO credentialsDTO);
}
