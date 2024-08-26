namespace Shopizy.Contracts.Category;

public record CreateCategoryRequest(string Name, Guid? ParentId);
