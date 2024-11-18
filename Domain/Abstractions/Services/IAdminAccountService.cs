using Domain.Entities;

namespace Domain.Abstractions.Services;

public interface IAdminAccountService
{
    public Task<List<AdminAccount>> GetAllAsync();
}
