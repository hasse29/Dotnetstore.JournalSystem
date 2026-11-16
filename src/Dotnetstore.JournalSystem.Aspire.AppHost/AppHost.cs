var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder
    .AddParameter(
        "postgres-password",
        new GenerateParameterDefault { MinLength = 32, Special = false },
        secret: true,
        persist: true);

var postgres = builder
    .AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var database = postgres
    .AddDatabase("journalsystem");

builder
    .AddProject<Projects.Dotnetstore_JournalSystem_Host>("api")
    .WithHttpHealthCheck("/health", endpointName: "https")
    .WithEnvironment("Database__EnsureCreatedOnStartup", "true")
    .WithReference(database)
    .WaitFor(database);

await builder
    .Build()
    .RunAsync()
    .ConfigureAwait(false);
