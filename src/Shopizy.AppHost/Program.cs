var builder = DistributedApplication.CreateBuilder(args);

// Cloud-native container resources for Shopizy ecosystem
var postgres = builder.AddPostgres("shopizy-postgres");

var redis = builder.AddRedis("shopizy-redis");

var rabbitmq = builder.AddRabbitMQ("shopizy-rabbitmq");

builder.Build().Run();
