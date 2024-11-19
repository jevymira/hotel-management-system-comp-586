using Domain.Entities;
using Domain.Models;

namespace Domain.Abstractions.Services;

public interface IAdminAccountService
{
    public Task<GetAdminAccountDTO> AddAsync(CreateAccountDTO accountDTO);
    public Task<GetAdminAccountDTO> GetAsync(string id);
    public Task<List<GetAdminAccountDTO>> GetAllAsync();
    public Task<string?> GetIDIfActiveValidCredentials(string username, string passwordHash);
    public Task UpdateDetailsAsync(string id, UpdateAdminAccountDTO updateDTO);
    public Task UpdatePasswordAsync(UpdatePasswordDTO credentialsDTO);
}
