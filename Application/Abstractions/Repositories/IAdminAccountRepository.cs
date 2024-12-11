using Application.Entities;
using Application.Models;

namespace Application.Abstractions.Repositories;

public interface IAdminAccountRepository
{
    public Task SaveAsync(AdminAccount adminAccount);
    public Task<AdminAccount> LoadAsync(string id);
    public Task<GetAdminAccountDTO> LoadForClientAsync(string id);
    public Task<List<GetAdminAccountDTO>> LoadAllForClientAsync();
    public Task<bool> QueryIfEmailExists(string email);
    // excludes account with included id in query
    public Task<bool> QueryIfEmailExists(string email, string id);
    public Task<AdminAccount?> QueryAccountByCredentialsIfActive(string email, string passwordHash);
    public Task<AdminAccount?> QueryAccountByCredentials(string email, string passwordHash);
    public Task<string?> QueryAuditIDByAccountID(string id);
    public Task UpdateDetailsAsync(AdminAccount account, string auditID, string updatedBy);
    public Task UpdatePasswordAsync(string id, string newPasswordHash);
}
