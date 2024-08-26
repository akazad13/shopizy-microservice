using ErrorOr;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Catelog.API.Services.Categories.Commands.UpdateCategory;

[Authorize(Permissions = Permissions.Category.Modify, Policies = Policy.SelfOrAdmin)]
public record UpdateCategoryCommand(Guid UserId, Guid CategoryId, string Name, Guid? ParentId)
    : IAuthorizeableRequest<ErrorOr<Category>>;
