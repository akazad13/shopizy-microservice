var builder = DistributedApplication.CreateBuilder(args);

// Cloud-native container resources for Shopizy ecosystem
var postgres = builder.AddPostgres("shopizy-postgres")
    .WithDataVolume();

var redis = builder.AddRedis("shopizy-redis");

var rabbitmq = builder.AddRabbitMQ("shopizy-rabbitmq");

builder.AddProject<Projects.Shopizy_IdentityService>("identity-service")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.AddProject<Projects.Shopizy_CatalogService>("catalog-service")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.Build().Run();
