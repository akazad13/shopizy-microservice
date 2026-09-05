using FluentAssertions;
using Moq;
using Shopizy.CatalogService.Application.Contracts;
using Shopizy.CatalogService.Application.Interfaces;
using Shopizy.CatalogService.Domain.Entities;
using Shopizy.CatalogService.Domain.ValueObjects;
using Shopizy.SharedKernel.Results;
using CatalogAppService = Shopizy.CatalogService.Application.Services.CatalogService;

namespace Shopizy.CatalogService.UnitTests.Application;

public class CatalogServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IBrandRepository> _brandRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly CatalogAppService _sut;

    public CatalogServiceTests()
    {
        _sut = new CatalogAppService(_categoryRepoMock.Object, _brandRepoMock.Object, _productRepoMock.Object);
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenParentDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.ExistsAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateCategoryRequest("Sub Category", "sub-category", null, parentId);

        // Act
        var result = await _sut.CreateCategoryAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Category.ParentNotFound");
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenSlugAlreadyExists_ReturnsConflict()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.SlugExistsAsync("existing-slug", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateCategoryRequest("Category", "existing-slug");

        // Act
        var result = await _sut.CreateCategoryAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Category.DuplicateSlug");
    }

    [Fact]
    public async Task CreateCategoryAsync_WithValidData_SavesAndReturnsSuccess()
    {
        // Arrange
        _categoryRepoMock.Setup(r => r.SlugExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateCategoryRequest("Accessories", "accessories", "Tech accessories");

        // Act
        var result = await _sut.CreateCategoryAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Accessories");
        result.Value.Slug.Should().Be("accessories");
        _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WhenCategoryNotFound_ReturnsNotFound()
    {
        // Arrange
        var catId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.GetByIdAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var request = new CreateProductRequest("Product", "product", "Desc", catId, Guid.NewGuid(), 99.99m);

        // Act
        var result = await _sut.CreateProductAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
    }

    [Fact]
    public async Task CreateProductAsync_WhenBrandNotFound_ReturnsNotFound()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.GetByIdAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Category.Create("Cat", "cat").Value);
        _brandRepoMock.Setup(r => r.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Brand?)null);

        var request = new CreateProductRequest("Product", "product", "Desc", catId, brandId, 99.99m);

        // Act
        var result = await _sut.CreateProductAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Brand.NotFound");
    }

    [Fact]
    public async Task CreateProductAsync_WhenSkuAlreadyExistsGlobally_ReturnsConflict()
    {
        // Arrange
        var catId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.GetByIdAsync(catId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Category.Create("Cat", "cat").Value);
        _brandRepoMock.Setup(r => r.GetByIdAsync(brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Brand.Create("Brand", "brand").Value);
        _productRepoMock.Setup(r => r.SlugExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _productRepoMock.Setup(r => r.SkuExistsAsync("EXISTING-SKU", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateProductRequest(
            "Product",
            "product",
            "Desc",
            catId,
            brandId,
            99.99m,
            "USD",
            null,
            [new ProductVariantDto("EXISTING-SKU", null, 99.99m, "USD", 10)]);

        // Act
        var result = await _sut.CreateProductAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.DuplicateSku");
    }
}
