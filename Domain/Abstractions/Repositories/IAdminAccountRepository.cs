using Domain.Entities;
using Domain.Models;

namespace Domain.Abstractions.Repositories;

public interface IAdminAccountRepository
{
    public Task SaveAsync(AdminAccount adminAccount);
    public Task<GetAdminAccountDTO> LoadAsync(string id);
    public Task<List<GetAdminAccountDTO>> LoadAllAsync();
    public Task<bool> QueryIfEmailExists(string email);
    public Task UpdateDetailsAsync(string id, UpdateAdminAccountDTO dto);
}
