namespace Application.Abstractions.Services;

public interface IJWTService
{
    public string IssueToken(string id);
}
