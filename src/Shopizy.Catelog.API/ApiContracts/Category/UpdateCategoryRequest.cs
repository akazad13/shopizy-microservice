namespace Shopizy.Catelog.API.ApiContracts.Category;

public record UpdateCategoryRequest(string Name, Guid? ParentId);
