namespace Shopizy.Security.User;

public interface ICurrentUserProvider
{
    CurrentUser? GetCurrentUser();
}