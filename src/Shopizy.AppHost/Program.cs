var builder = DistributedApplication.CreateBuilder(args);

// Cloud-native container resources for Shopizy ecosystem
var postgres = builder.AddPostgres("shopizy-postgres")
    .WithDataVolume();

var redis = builder.AddRedis("shopizy-redis");

var rabbitmq = builder.AddRabbitMQ("shopizy-rabbitmq");

var identityDb = postgres.AddDatabase("identitydb");
var catalogDb = postgres.AddDatabase("catalogdb");

builder.AddProject<Projects.Shopizy_IdentityService>("identity-service")
    .WithReference(identityDb)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.AddProject<Projects.Shopizy_CatalogService>("catalog-service")
    .WithReference(catalogDb)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.Build().Run();
