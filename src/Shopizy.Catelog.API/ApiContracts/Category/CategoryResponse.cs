namespace Shopizy.Catelog.API.ApiContracts.Category;

public record CategoryResponse(Guid Id, string Name, Guid? ParentId);
