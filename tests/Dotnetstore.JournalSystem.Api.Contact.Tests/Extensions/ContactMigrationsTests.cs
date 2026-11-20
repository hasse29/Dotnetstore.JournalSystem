using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Extensions;

public sealed class ContactMigrationsTests(SqlServerFixture fixture)
{
    private static ServiceProvider CreateProvider(string connectionString)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = connectionString;
        builder.RegisterContactServices();
        return builder.Services.BuildServiceProvider();
    }

    [Fact]
    public async Task InitializeRecordsTheMigrationInTheContactSchema()
    {
        // Arrange
        await using var provider = CreateProvider(fixture.CreateDatabaseConnectionString());

        // Act
        await provider.InitializeContactDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContactDataContext>();
        var applied = await context.Database
            .SqlQuery<string>($"SELECT MigrationId AS [Value] FROM [Contact].[__EFMigrationsHistory]")
            .ToListAsync(TestContext.Current.CancellationToken);
        applied.ShouldHaveSingleItem().ShouldEndWith("_InitialCreate");
        await context.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ModelHasNoChangesThatAreMissingFromTheMigrations()
    {
        // Arrange
        await using var provider = CreateProvider(fixture.CreateDatabaseConnectionString());
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContactDataContext>();

        // Act
        var hasPendingChanges = context.Database.HasPendingModelChanges();

        // Assert
        hasPendingChanges.ShouldBeFalse("add a migration: dotnet ef migrations add <Name> --project src/Dotnetstore.JournalSystem.Api.Contact --context ContactDataContext --output-dir Data/Migrations");
    }
}
