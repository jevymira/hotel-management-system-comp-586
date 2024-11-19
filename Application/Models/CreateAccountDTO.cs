namespace Application.Models;

public class CreateAccountDTO
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
}
