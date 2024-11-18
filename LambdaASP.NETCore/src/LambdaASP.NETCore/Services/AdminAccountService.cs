using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;
using Domain.Models;
using System.Linq.Expressions;
using System.Security.Authentication;

namespace LambdaASP.NETCore.Services;

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
        AdminAccount adminAccount = new AdminAccount
        {
            AdminID = IdGenerator.Get6CharBase62(),
            FullName = accountDTO.Name,
            Email = accountDTO.Email,
            PasswordHash = accountDTO.PasswordHash,
            AccountStatus = "Active"
        };
        await _adminAccountRepository.SaveAsync(adminAccount);
        return new GetAdminAccountDTO 
        {
            AdminID = adminAccount.AdminID,
            FullName = adminAccount.FullName,
            Email = adminAccount.Email,
            AccountStatus = "Active"
        };
    }

    public async Task<GetAdminAccountDTO> GetAsync(string id)
    {
        var account = await _adminAccountRepository.LoadAsync(id);
        if (account == null)
            throw new KeyNotFoundException($"No account exists with ID {id}.");
        return account;
    }

    public async Task<List<GetAdminAccountDTO>> GetAllAsync()
    {
        return await _adminAccountRepository.LoadAllAsync();
    }

    public async Task UpdateDetailsAsync(string id, UpdateAdminAccountDTO updateDTO)
    {
        if ((await _adminAccountRepository.LoadAsync(id)) == null)
            throw new KeyNotFoundException($"No account exists with ID {id}.");
            // otherwise, DynamoDB will create new item

        if (await _adminAccountRepository.QueryIfEmailExists(updateDTO.Email))
            throw new ArgumentException($"Email {updateDTO.Email} is already in use.");

        await _adminAccountRepository.UpdateDetailsAsync(id, updateDTO);
    }

    public async Task UpdatePasswordAsync(UpdatePasswordDTO credentialsDTO)
    {
        AdminAccount account;
        try
        {
            account = await _adminAccountRepository.QueryAccountByCredentials(credentialsDTO);
        }
        catch (InvalidOperationException) // sequence contains no elements
        {
            throw new InvalidCredentialException("Email or password invalid.");
        }
        await _adminAccountRepository.UpdatePasswordAsync(account.AdminID, credentialsDTO.NewPasswordHash);
    }
}
