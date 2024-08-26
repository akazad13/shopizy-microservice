namespace Shopizy.Contracts.Category;

public record UpdateCategoryRequest(string Name, Guid? ParentId);
