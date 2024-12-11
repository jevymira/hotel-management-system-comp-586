using Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services;

/// <summary>
/// Service to generate JSON Web Tokens for system auth.
/// </summary>
public class JWTService : IJWTService
{
    private readonly IConfiguration _config;

    public JWTService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Generate JSON Web Token using details from the configuration file,
    /// additionally storing the Admin Account ID in Sub.
    /// </summary>
    /// <param name="id">Admin Account ID</param>
    /// <returns>Encoded Token</returns>
    public string IssueToken(string id)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, id) };
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_config["Jwt:Issuer"],
          _config["Jwt:Issuer"],
          claims: claims,
          expires: DateTime.Now.AddMinutes(120),
          signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token));
    }
}
