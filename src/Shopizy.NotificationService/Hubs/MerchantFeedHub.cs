using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Shopizy.NotificationService.Hubs;

[Authorize(Roles = "StoreAdmin")]
public sealed class MerchantFeedHub : Hub
{
}
