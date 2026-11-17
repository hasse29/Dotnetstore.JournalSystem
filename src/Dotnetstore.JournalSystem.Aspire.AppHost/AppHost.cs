var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder
    .AddParameter(
        "sql-password",
        new GenerateParameterDefault { MinLength = 32, Special = false, MinLower = 1, MinUpper = 1, MinNumeric = 1 },
        secret: true,
        persist: true);

var sql = builder
    .AddSqlServer("sql", password: sqlPassword)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = sql
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
