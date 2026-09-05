using Shopizy.SharedKernel.Domain;

namespace Shopizy.CatalogService.Domain.Events;

public sealed record ProductCreatedDomainEvent(Guid ProductId, string Name, string Slug) : IDomainEvent;

public sealed record ProductUpdatedDomainEvent(Guid ProductId, string Name, int NewVersion) : IDomainEvent;

public sealed record ProductStockUpdatedDomainEvent(Guid ProductId, Guid VariantId, string Sku, int OldStock, int NewStock) : IDomainEvent;
