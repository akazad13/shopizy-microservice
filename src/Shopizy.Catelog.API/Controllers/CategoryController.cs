using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shopizy.Catelog.API.Services.Categories.Commands.CreateCategory;
using Shopizy.Catelog.API.Services.Categories.Commands.DeleteCategory;
using Shopizy.Catelog.API.Services.Categories.Commands.UpdateCategory;
using Shopizy.Catelog.API.Services.Categories.Queries.GetCategory;
using Shopizy.Catelog.API.Services.Categories.Queries.ListCategories;
using Shopizy.Contracts.Category;

namespace Shopizy.Catelog.API.Controllers;

[Route("api/v1.0")]
public class CategoryController(ISender _mediator, IMapper _mapper) : ApiController
{
    [HttpGet("categories")]
    public async Task<IActionResult> Get()
    {
        var query = new ListCategoriesQuery();
        var result = await _mediator.Send(query);

        return result.Match(category => Ok(_mapper.Map<List<CategoryResponse>>(category)), Problem);
    }

    [HttpGet("categories/{categoryId:guid}")]
    public async Task<IActionResult> GetCategory(Guid categoryId)
    {
        var query = _mapper.Map<GetCategoryQuery>(categoryId);
        var result = await _mediator.Send(query);

        return result.Match(category => Ok(_mapper.Map<CategoryResponse>(category)), Problem);
    }

    [HttpPost("users/{userId:guid}/categories")]
    public async Task<IActionResult> CreateCategory(Guid userId, CreateCategoryRequest request)
    {
        var command = _mapper.Map<CreateCategoryCommand>((userId, request));
        var result = await _mediator.Send(command);

        return result.Match(category => Ok(_mapper.Map<CategoryResponse>(category)), Problem);
    }

    [HttpPatch("users/{userId:guid}/categories/{categoryId:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid userId,
        Guid categoryId,
        UpdateCategoryRequest request
    )
    {
        var command = _mapper.Map<UpdateCategoryCommand>((userId, categoryId, request));
        var result = await _mediator.Send(command);

        return result.Match(category => Ok(_mapper.Map<CategoryResponse>(category)), Problem);
    }

    [HttpDelete("users/{userId:guid}/categories/{categoryId:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid userId, Guid categoryId)
    {
        var command = _mapper.Map<DeleteCategoryCommand>((userId, categoryId));
        var result = await _mediator.Send(command);

        return result.Match(category => Ok(_mapper.Map<Success>(category)), Problem);
    }
}

