namespace Domain.Models;

public class UpdatePasswordDTO
{
    public required string Email { get; set; }
    public required string OldPasswordHash { get; set; }
    public required string NewPasswordHash { get; set; }
}
