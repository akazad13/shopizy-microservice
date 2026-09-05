using Shopizy.IdentityService.Domain.Enums;
using Shopizy.SharedKernel.Domain;

namespace Shopizy.IdentityService.Domain.Events;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role) : IDomainEvent;
