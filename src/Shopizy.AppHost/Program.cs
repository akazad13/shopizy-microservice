var builder = DistributedApplication.CreateBuilder(args);

// Cloud-native container resources for Shopizy ecosystem
var postgres = builder.AddPostgres("shopizy-postgres")
    .WithDataVolume();

var redis = builder.AddRedis("shopizy-redis");

var rabbitmq = builder.AddRabbitMQ("shopizy-rabbitmq");

var identityDb = postgres.AddDatabase("identitydb");
var catalogDb = postgres.AddDatabase("catalogdb");
var orderDb = postgres.AddDatabase("orderdb");
var paymentDb = postgres.AddDatabase("paymentdb");
var promotionDb = postgres.AddDatabase("promotiondb");
var shippingDb = postgres.AddDatabase("shippingdb");

builder.AddProject<Projects.Shopizy_IdentityService>("identity-service")
    .WithReference(identityDb)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.AddProject<Projects.Shopizy_CatalogService>("catalog-service")
    .WithReference(catalogDb)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.AddProject<Projects.Shopizy_CartService>("cart-service")
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.AddProject<Projects.Shopizy_OrderService>("order-service")
    .WithReference(orderDb)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.AddProject<Projects.Shopizy_PaymentService>("payment-service")
    .WithReference(paymentDb)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.AddProject<Projects.Shopizy_SearchService>("search-service")
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.AddProject<Projects.Shopizy_PromotionService>("promotion-service")
    .WithReference(promotionDb)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.AddProject<Projects.Shopizy_ShippingService>("shipping-service")
    .WithReference(shippingDb)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.Build().Run();
