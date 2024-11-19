using Application.Abstractions.Services;
using Application.Helpers.Services;
using Application.Models;
using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;

namespace Application.Services;

public class AdminAccountService : IAdminAccountService
{
    IAdminAccountRepository _adminAccountRepository;

    public AdminAccountService(IAdminAccountRepository adminAccountRepository)
    {
        _adminAccountRepository = adminAccountRepository;
    }

    public async Task<GetAdminAccountDTO> AddAsync(CreateAccountDTO accountDTO)
    {
        if (await _adminAccountRepository.QueryIfEmailExists(accountDTO.Email))
            throw new ArgumentException($"Email {accountDTO.Email} is already in use.");

        AdminAccount account = new AdminAccount
        {
            AdminID = IdGenerator.Get6CharBase62(),
            FullName = accountDTO.Name,
            Email = accountDTO.Email,
            PasswordHash = accountDTO.PasswordHash,
        };
        account.Activate();

        await _adminAccountRepository.SaveAsync(account);

        return new GetAdminAccountDTO 
        {
            AdminID = account.AdminID,
            FullName = account.FullName,
            Email = account.Email,
            AccountStatus = account.AccountStatus
        };
    }

    public async Task<GetAdminAccountDTO> GetAsync(string id)
    {
        var account = await _adminAccountRepository.LoadAsync(id);
        return account;
    }

    public async Task<List<GetAdminAccountDTO>> GetAllAsync()
    {
        return await _adminAccountRepository.LoadAllAsync();
    }

    public async Task<string?> GetIDIfActiveValidCredentials(string email, string passwordHash)
    {
        var account = await _adminAccountRepository.QueryAccountByCredentialsIfActive(email, passwordHash);
        if (account == null)
            return null;
        return account.AdminID;
    }

    public async Task UpdateDetailsAsync(string id, UpdateAdminAccountDTO dto)
    {
        if ((await _adminAccountRepository.LoadAsync(id)) == null)
            throw new KeyNotFoundException($"No account exists with ID {id}.");
            // otherwise, DynamoDB will create new item

        if (await _adminAccountRepository.QueryIfEmailExists(dto.Email))
            throw new ArgumentException($"Email {dto.Email} is already in use.");

        await _adminAccountRepository.UpdateDetailsAsync(id, dto.FullName, dto.Email, dto.AccountStatus);
    }

    // TODO: REFACTOR from exceptions
    public async Task<bool> UpdatePasswordAsync(UpdatePasswordDTO credentialsDTO)
    {
        AdminAccount? account = await _adminAccountRepository.QueryAccountByCredentials(
                                credentialsDTO.Email, credentialsDTO.OldPasswordHash);
        if (account == null)
            return false;

        await _adminAccountRepository.UpdatePasswordAsync(account.AdminID, credentialsDTO.NewPasswordHash);
        return true;
    }
}
