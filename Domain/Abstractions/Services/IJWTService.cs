namespace Application.Abstractions.Services;

/// <summary>
/// Service to generate JSON Web Tokens for system auth.
/// </summary>
public interface IJWTService
{
    /// <summary>
    /// Generate JSON Web Token, storing the Admin Account ID in Sub.
    /// </summary>
    /// <param name="id">Admin Account ID</param>
    /// <returns>Encoded Token</returns>
    public string IssueToken(string id);
}
