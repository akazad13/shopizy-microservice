using ErrorOr;
using Shopizy.Catelog.API.Aggregates.Categories;
using Shopizy.Security.Permissions;
using Shopizy.Security.Policies;
using Shopizy.Security.Request;

namespace Shopizy.Catelog.API.Services.Categories.Commands.CreateCategory;

[Authorize(Permissions = Permissions.Category.Create, Policies = Policy.SelfOrAdmin)]
public record CreateCategoryCommand(Guid UserId, string Name, Guid? ParentId)
    : IAuthorizeableRequest<ErrorOr<Category>>;

