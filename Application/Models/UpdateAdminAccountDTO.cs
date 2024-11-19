namespace Application.Models;

public class UpdateAdminAccountDTO
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string AccountStatus { get; set; }
}
