namespace Shopizy.Security.TokenGenerator;

public interface IJwtTokenGenerator
{
    string GenerateToken(
        Guid userId,
        string firstName,
        string LastName,
        string phone,
        IList<string> roles,
        IList<string> Permissions
    );
}
