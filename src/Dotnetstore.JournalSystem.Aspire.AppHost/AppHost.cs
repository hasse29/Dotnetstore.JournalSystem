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

var api = builder
    .AddDotnetProject("api", "../Dotnetstore.JournalSystem.Host/Dotnetstore.JournalSystem.Host.csproj")
    .WithHttpHealthCheck("/health", endpointName: "https")
    .WithEnvironment("Database__MigrateOnStartup", "true")
    .WithReference(database)
    .WaitFor(database);

builder
    .AddDotnetProject("webui", "../Dotnetstore.JournalSystem.FrontEnd.WebUi/Dotnetstore.JournalSystem.FrontEnd.WebUi.csproj")
    .WithEnvironment("JournalApi__BaseAddress", "https+http://api")
    .WithReference(api)
    .WaitFor(api);

await builder
    .Build()
    .RunAsync()
    .ConfigureAwait(false);
