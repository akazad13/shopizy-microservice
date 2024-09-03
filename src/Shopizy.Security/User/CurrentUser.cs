namespace Shopizy.Security.User;

public record CurrentUser(
    Guid Id,
    string FirstName,
    string LastName,
    string Phone,
    IList<string> Permissions,
    IList<string> Roles
);
