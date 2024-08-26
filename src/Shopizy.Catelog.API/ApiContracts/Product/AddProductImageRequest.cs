using Microsoft.AspNetCore.Http;

namespace Shopizy.Contracts.Product;

public record AddProductImageRequest(IFormFile File);
