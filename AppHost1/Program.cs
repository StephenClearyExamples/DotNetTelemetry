using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddResource(new ProjectResource("api")).WithAnnotation(new WebApi());

builder.AddRabbitMQ("queue", builder.AddParameter("rabbitmq-username"), builder.AddParameter("rabbitmq-password"), 5672)
	.WithManagementPlugin();

builder.Build().Run();
