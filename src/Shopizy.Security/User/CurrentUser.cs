namespace Shopizy.Security.User;

public record CurrentUser(
    Guid Id,
    string FirstName,
    string LastName,
    string Phone,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Roles
);
