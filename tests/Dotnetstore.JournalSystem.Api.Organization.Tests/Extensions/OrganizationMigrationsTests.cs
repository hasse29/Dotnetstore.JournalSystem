using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Extensions;

public sealed class OrganizationMigrationsTests(SqlServerFixture fixture)
{
    private static ServiceProvider CreateProvider(string connectionString)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = connectionString;
        builder.RegisterOrganizationServices();
        return builder.Services.BuildServiceProvider();
    }

    private static async Task DeleteDatabaseAsync(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<OrganizationDataContext>().Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task InitializeCreatesTheTablesAndRecordsTheMigrationInTheOrganizationSchema()
    {
        // Arrange
        await using var provider = CreateProvider(fixture.CreateDatabaseConnectionString());

        // Act
        await provider.InitializeOrganizationDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrganizationDataContext>();
        (await context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
        var applied = await context.Database
            .SqlQuery<string>($"SELECT MigrationId AS [Value] FROM [Organization].[__EFMigrationsHistory] ORDER BY MigrationId")
            .ToListAsync(TestContext.Current.CancellationToken);
        applied.Count.ShouldBe(4);
        applied[0].ShouldEndWith("_InitialCreate");
        applied[1].ShouldEndWith("_AddAccessRequests");
        applied[2].ShouldEndWith("_AddPersonDetails");
        applied[3].ShouldEndWith("_AddOwnCompanyImages");
        await DeleteDatabaseAsync(provider);
    }

    [Fact]
    public async Task InitializeCanBeCalledMoreThanOnce()
    {
        // Arrange
        await using var provider = CreateProvider(fixture.CreateDatabaseConnectionString());
        await provider.InitializeOrganizationDatabaseAsync(TestContext.Current.CancellationToken);

        // Act
        var action = () => provider.InitializeOrganizationDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        await Should.NotThrowAsync(action);
        await DeleteDatabaseAsync(provider);
    }

    [Fact]
    public async Task InitializeFailsForAnUnreachableDatabase()
    {
        // Arrange
        await using var provider = CreateProvider("Server=localhost,1;Database=journalsystem;User Id=u;Password=p;Connect Timeout=1;TrustServerCertificate=True");

        // Act
        var action = () => provider.InitializeOrganizationDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<Exception>(action);
    }

    [Fact]
    public async Task ModelHasNoChangesThatAreMissingFromTheMigrations()
    {
        // Arrange
        await using var provider = CreateProvider(fixture.CreateDatabaseConnectionString());
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrganizationDataContext>();

        // Act
        var hasPendingChanges = context.Database.HasPendingModelChanges();

        // Assert
        hasPendingChanges.ShouldBeFalse("add a migration: dotnet ef migrations add <Name> --project src/Dotnetstore.JournalSystem.Api.Organization --context OrganizationDataContext --output-dir Data/Migrations");
    }
}
