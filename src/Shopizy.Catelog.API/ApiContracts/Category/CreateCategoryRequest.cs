namespace Shopizy.Catelog.API.ApiContracts.Category;

public record CreateCategoryRequest(string Name, Guid? ParentId);
