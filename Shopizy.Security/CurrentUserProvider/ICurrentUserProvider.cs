namespace Shopizy.Security.CurrentUserProvider;

public interface ICurrentUserProvider
{
    CurrentUser? GetCurrentUser();
}