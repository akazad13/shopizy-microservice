using ErrorOr;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Catelog.API.Services.Categories.Commands.DeleteCategory;

[Authorize(Permissions = Permissions.Category.Delete, Policies = Policy.SelfOrAdmin)]
public record DeleteCategoryCommand(Guid UserId, Guid CategoryId)
    : IAuthorizeableRequest<ErrorOr<Success>>;
