using Domain.Entities;

namespace Domain.Abstractions.Repositories;

public interface IAdminAccountRepository
{
    public Task<List<AdminAccount>> LoadAccountsAsync();
}
