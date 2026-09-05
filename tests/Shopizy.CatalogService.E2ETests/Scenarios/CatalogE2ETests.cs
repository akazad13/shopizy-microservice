using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Shopizy.CatalogService.Application.Contracts;
using Shopizy.CatalogService.E2ETests.Fixtures;

namespace Shopizy.CatalogService.E2ETests.Scenarios;

public sealed class CatalogE2ETests : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CatalogE2ETests(CatalogWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string uri, string role)
    {
        var request = new HttpRequestMessage(method, uri);
        var token = CatalogWebApplicationFactory.GenerateJwtToken(role);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task E2E_Scenario01_StoreAdminCategoryHierarchyAndBrandCreation()
    {
        // Step 1: StoreAdmin creates parent category 'Electronics'
        var parentCatReq = new CreateCategoryRequest("Electronics", "electronics", "All electronic gadgets");
        using var postParentMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/categories", "StoreAdmin");
        postParentMsg.Content = JsonContent.Create(parentCatReq);

        var parentResponse = await _client.SendAsync(postParentMsg);
        parentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var parentCategory = await parentResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        parentCategory.Should().NotBeNull();
        parentCategory!.Name.Should().Be("Electronics");

        // Step 2: StoreAdmin creates child category 'Audio' referencing 'Electronics'
        var childCatReq = new CreateCategoryRequest("Audio", "audio", "Headphones, speakers, sound systems", parentCategory.Id);
        using var postChildMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/categories", "StoreAdmin");
        postChildMsg.Content = JsonContent.Create(childCatReq);

        var childResponse = await _client.SendAsync(postChildMsg);
        childResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var childCategory = await childResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        childCategory.Should().NotBeNull();
        childCategory!.ParentCategoryId.Should().Be(parentCategory.Id);

        // Step 3: StoreAdmin creates brand 'AudioTech'
        var brandReq = new CreateBrandRequest("AudioTech", "audiotech", "Premium Audio Manufacturer", "https://audiotech.example.com", "https://audiotech.example.com/logo.png");
        using var postBrandMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/brands", "StoreAdmin");
        postBrandMsg.Content = JsonContent.Create(brandReq);

        var brandResponse = await _client.SendAsync(postBrandMsg);
        brandResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var brand = await brandResponse.Content.ReadFromJsonAsync<BrandResponse>();
        brand.Should().NotBeNull();
        brand!.Name.Should().Be("AudioTech");

        // Step 4: Public user retrieves category tree and verifies hierarchy
        var getCatResponse = await _client.GetAsync($"/api/v1/catalog/categories/{parentCategory.Id}");
        getCatResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedParent = await getCatResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        fetchedParent.Should().NotBeNull();
        fetchedParent!.SubCategories.Should().Contain(c => c.Name == "Audio");
    }

    [Fact]
    public async Task E2E_Scenario02_StoreAdminProductWithVariantsAndGallery()
    {
        // Prerequisites: Create Category and Brand
        var catReq = new CreateCategoryRequest("Wearables", $"wearables-{Guid.NewGuid():N}");
        using var catMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/categories", "StoreAdmin");
        catMsg.Content = JsonContent.Create(catReq);
        var catRes = await _client.SendAsync(catMsg);
        var category = await catRes.Content.ReadFromJsonAsync<CategoryResponse>();

        var brandReq = new CreateBrandRequest("WearTech", $"weartech-{Guid.NewGuid():N}");
        using var brandMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/brands", "StoreAdmin");
        brandMsg.Content = JsonContent.Create(brandReq);
        var brandRes = await _client.SendAsync(brandMsg);
        var brand = await brandRes.Content.ReadFromJsonAsync<BrandResponse>();

        // Step 1: StoreAdmin creates product with 2 variants & image gallery
        var sku1 = $"WT-PRO-BLK-{Guid.NewGuid():N}";
        var sku2 = $"WT-PRO-SLV-{Guid.NewGuid():N}";

        var productReq = new CreateProductRequest(
            Name: "WearTech SmartWatch Pro",
            Slug: $"weartech-smartwatch-pro-{Guid.NewGuid():N}",
            Description: "Flagship smart watch with health tracking.",
            CategoryId: category!.Id,
            BrandId: brand!.Id,
            BasePrice: 249.99m,
            Currency: "USD",
            Images:
            [
                new ProductImageDto("https://cdn.shopizy.test/sw-main.png", "Main Watch Front", 1, true),
                new ProductImageDto("https://cdn.shopizy.test/sw-side.png", "Watch Profile", 2, false)
            ],
            Variants:
            [
                new ProductVariantDto(sku1, "98765432101", 249.99m, "USD", 40, new Dictionary<string, string> { { "Color", "Midnight Black" } }),
                new ProductVariantDto(sku2, "98765432102", 269.99m, "USD", 15, new Dictionary<string, string> { { "Color", "Platinum Silver" } })
            ]);

        using var prodMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/products", "StoreAdmin");
        prodMsg.Content = JsonContent.Create(productReq);

        var prodResponse = await _client.SendAsync(prodMsg);
        prodResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var product = await prodResponse.Content.ReadFromJsonAsync<ProductDetailResponse>();
        product.Should().NotBeNull();
        product!.Name.Should().Be("WearTech SmartWatch Pro");
        product.Images.Should().HaveCount(2);
        product.Variants.Should().HaveCount(2);
        product.Variants.Should().Contain(v => v.Sku == sku1.ToUpperInvariant() && v.StockQuantity == 40);
        product.Variants.Should().Contain(v => v.Sku == sku2.ToUpperInvariant() && v.StockQuantity == 15);
    }

    [Fact]
    public async Task E2E_Scenario03_CustomerPublicBrowsingFilteringSortingPagination()
    {
        // Step 1: Query catalog anonymously with filters
        var response = await _client.GetAsync("/api/v1/catalog/products?page=1&pageSize=10&sortBy=price_asc");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var paged = await response.Content.ReadFromJsonAsync<PagedResult<ProductListResponse>>();
        paged.Should().NotBeNull();
        paged!.Page.Should().Be(1);
        paged.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task E2E_Scenario04_CustomerProductDetailAndVariantStockInspection()
    {
        // Arrange: Create Category, Brand, and Product
        var catReq = new CreateCategoryRequest("Display", $"display-{Guid.NewGuid():N}");
        using var catMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/categories", "StoreAdmin");
        catMsg.Content = JsonContent.Create(catReq);
        var catRes = await _client.SendAsync(catMsg);
        var category = await catRes.Content.ReadFromJsonAsync<CategoryResponse>();

        var brandReq = new CreateBrandRequest("ScreenTech", $"screentech-{Guid.NewGuid():N}");
        using var brandMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/brands", "StoreAdmin");
        brandMsg.Content = JsonContent.Create(brandReq);
        var brandRes = await _client.SendAsync(brandMsg);
        var brand = await brandRes.Content.ReadFromJsonAsync<BrandResponse>();

        var sku = $"DISP-4K-{Guid.NewGuid():N}";
        var productReq = new CreateProductRequest(
            Name: "UltraHD 4K Monitor",
            Slug: $"ultrahd-4k-monitor-{Guid.NewGuid():N}",
            Description: "32 inch 4K IPS display.",
            CategoryId: category!.Id,
            BrandId: brand!.Id,
            BasePrice: 499.99m,
            Currency: "USD",
            Variants: [new ProductVariantDto(sku, null, 499.99m, "USD", 12)]);

        using var prodMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/products", "StoreAdmin");
        prodMsg.Content = JsonContent.Create(productReq);
        var prodRes = await _client.SendAsync(prodMsg);
        var createdProduct = await prodRes.Content.ReadFromJsonAsync<ProductDetailResponse>();

        // Act: Anonymous client inspects product detail
        var detailRes = await _client.GetAsync($"/api/v1/catalog/products/{createdProduct!.Id}");
        detailRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await detailRes.Content.ReadFromJsonAsync<ProductDetailResponse>();
        detail.Should().NotBeNull();
        detail!.Id.Should().Be(createdProduct.Id);
        detail.Variants.Should().ContainSingle(v => v.Sku == sku.ToUpperInvariant() && v.StockQuantity == 12 && v.IsInStock);
    }

    [Fact]
    public async Task E2E_Scenario05_RoleBasedAccessControlSecurityEnforcement()
    {
        var dummyProductReq = new CreateProductRequest("Dummy", "dummy", "desc", Guid.NewGuid(), Guid.NewGuid(), 10m);

        // Case 1: Anonymous -> 401 Unauthorized
        var anonRes = await _client.PostAsJsonAsync("/api/v1/catalog/products", dummyProductReq);
        anonRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Case 2: Customer Role -> 403 Forbidden
        using var customerMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/products", "Customer");
        customerMsg.Content = JsonContent.Create(dummyProductReq);
        var customerRes = await _client.SendAsync(customerMsg);
        customerRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task E2E_Scenario06_OptimisticConcurrencyProtectionOnProductUpdate()
    {
        // Arrange: Create Category, Brand, and Product
        var catReq = new CreateCategoryRequest("Storage", $"storage-{Guid.NewGuid():N}");
        using var catMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/categories", "StoreAdmin");
        catMsg.Content = JsonContent.Create(catReq);
        var catRes = await _client.SendAsync(catMsg);
        var category = await catRes.Content.ReadFromJsonAsync<CategoryResponse>();

        var brandReq = new CreateBrandRequest("FlashCorp", $"flashcorp-{Guid.NewGuid():N}");
        using var brandMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/brands", "StoreAdmin");
        brandMsg.Content = JsonContent.Create(brandReq);
        var brandRes = await _client.SendAsync(brandMsg);
        var brand = await brandRes.Content.ReadFromJsonAsync<BrandResponse>();

        var slug = $"fast-ssd-1tb-{Guid.NewGuid():N}";
        var productReq = new CreateProductRequest(
            Name: "Fast SSD 1TB",
            Slug: slug,
            Description: "NVMe PCIe 4.0 SSD",
            CategoryId: category!.Id,
            BrandId: brand!.Id,
            BasePrice: 109.99m);

        using var prodMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/products", "StoreAdmin");
        prodMsg.Content = JsonContent.Create(productReq);
        var prodRes = await _client.SendAsync(prodMsg);
        var product = await prodRes.Content.ReadFromJsonAsync<ProductDetailResponse>();
        product!.Version.Should().Be(1);

        // Step 1: First update succeeds (Version becomes 2)
        var update1 = new UpdateProductRequest(
            Name: "Fast SSD 1TB Edition 2",
            Slug: slug,
            Description: "Updated description",
            CategoryId: category.Id,
            BrandId: brand.Id,
            BasePrice: 119.99m,
            Currency: "USD",
            ExpectedVersion: 1);

        using var update1Msg = CreateAuthorizedRequest(HttpMethod.Put, $"/api/v1/catalog/products/{product.Id}", "StoreAdmin");
        update1Msg.Content = JsonContent.Create(update1);
        var update1Res = await _client.SendAsync(update1Msg);
        update1Res.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedProduct = await update1Res.Content.ReadFromJsonAsync<ProductDetailResponse>();
        updatedProduct!.Version.Should().Be(2);

        // Step 2: Second concurrent update using stale Version = 1 receives 409 Conflict
        var update2Stale = new UpdateProductRequest(
            Name: "Fast SSD 1TB Concurrent",
            Slug: slug,
            Description: "Conflicting description",
            CategoryId: category.Id,
            BrandId: brand.Id,
            BasePrice: 129.99m,
            Currency: "USD",
            ExpectedVersion: 1); // Stale!

        using var update2Msg = CreateAuthorizedRequest(HttpMethod.Put, $"/api/v1/catalog/products/{product.Id}", "StoreAdmin");
        update2Msg.Content = JsonContent.Create(update2Stale);
        var update2Res = await _client.SendAsync(update2Msg);
        update2Res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await update2Res.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Product.ConcurrencyConflict");
    }

    [Fact]
    public async Task E2E_Scenario07_IdempotentProductCreationViaIdempotencyKey()
    {
        // Arrange: Create Category and Brand
        var catReq = new CreateCategoryRequest("Cables", $"cables-{Guid.NewGuid():N}");
        using var catMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/categories", "StoreAdmin");
        catMsg.Content = JsonContent.Create(catReq);
        var catRes = await _client.SendAsync(catMsg);
        var category = await catRes.Content.ReadFromJsonAsync<CategoryResponse>();

        var brandReq = new CreateBrandRequest("WireWorks", $"wireworks-{Guid.NewGuid():N}");
        using var brandMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/brands", "StoreAdmin");
        brandMsg.Content = JsonContent.Create(brandReq);
        var brandRes = await _client.SendAsync(brandMsg);
        var brand = await brandRes.Content.ReadFromJsonAsync<BrandResponse>();

        var idempotencyKey = $"idempotency-{Guid.NewGuid()}";
        var productReq = new CreateProductRequest(
            Name: "Braided Thunderbolt Cable",
            Slug: $"braided-thunderbolt-cable-{Guid.NewGuid():N}",
            Description: "2m high speed cable",
            CategoryId: category!.Id,
            BrandId: brand!.Id,
            BasePrice: 39.99m);

        // Step 1: Initial POST with Idempotency-Key
        using var firstMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/products", "StoreAdmin");
        firstMsg.Headers.Add("Idempotency-Key", idempotencyKey);
        firstMsg.Content = JsonContent.Create(productReq);

        var firstRes = await _client.SendAsync(firstMsg);
        firstRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var firstProduct = await firstRes.Content.ReadFromJsonAsync<ProductDetailResponse>();

        // Step 2: Replay identical request with identical Idempotency-Key
        using var secondMsg = CreateAuthorizedRequest(HttpMethod.Post, "/api/v1/catalog/products", "StoreAdmin");
        secondMsg.Headers.Add("Idempotency-Key", idempotencyKey);
        secondMsg.Content = JsonContent.Create(productReq);

        var secondRes = await _client.SendAsync(secondMsg);
        secondRes.StatusCode.Should().Be(HttpStatusCode.Created);
        secondRes.Headers.Contains("X-Cache-Lookup").Should().BeTrue();
        secondRes.Headers.GetValues("X-Cache-Lookup").Should().Contain("HIT");

        var secondProduct = await secondRes.Content.ReadFromJsonAsync<ProductDetailResponse>();
        secondProduct.Should().NotBeNull();
        secondProduct!.Id.Should().Be(firstProduct!.Id);
    }
}
