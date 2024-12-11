using Domain.Entities;
using Domain.Models;

namespace Domain.Abstractions.Repositories;

/// <summary>
/// Encapsulates the logic for the retrieval/persistence of admin accounts.
/// </summary>
public interface IAdminAccountRepository
{
    public Task SaveAsync(AdminAccount adminAccount);
    public Task<AdminAccount> LoadAsync(string id);

    /// <summary>
    /// Retrieve the account, specified by its ID, but with its password hash omitted.
    /// </summary>
    /// <param name="id">ID of account to retrieve.</param>
    public Task<GetAdminAccountDTO> LoadForClientAsync(string id);
    public Task<List<GetAdminAccountDTO>> LoadAllForClientAsync();
    public Task<bool> QueryIfEmailExists(string email);

    /// <summary>
    /// Query whether an email exists, excluding the specified account (by ID).
    /// </summary>
    /// <param name="email">Email to query on.</param>
    /// <param name="id">ID of account to ignore.</param>
    /// <returns>True if email exists outside of ignored account.</returns>
    public Task<bool> QueryIfEmailExists(string email, string id);

    /// <summary>
    /// Query account with email/password combination and which additionaly is Active.
    /// </summary>
    /// <param name="email">Admin account email.</param>
    /// <param name="passwordHash">Admin account password hash.</param>
    public Task<AdminAccount?> QueryAccountByCredentialsIfActive(string email, string passwordHash);
    public Task<AdminAccount?> QueryAccountByCredentials(string email, string passwordHash);
    public Task<string?> QueryAuditIDByAccountID(string id);
    public Task UpdateDetailsAsync(AdminAccount account, string auditID, string updatedBy);
    public Task UpdatePasswordAsync(string id, string newPasswordHash);
}
