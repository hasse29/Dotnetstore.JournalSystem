using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Extensions;

public sealed class HostApplicationBuilderExtensionsTests
{
    private const string ConnectionString = "Host=localhost;Port=1;Database=journalsystem;Username=u;Password=p;Timeout=1";

    private static HostApplicationBuilder CreateBuilder(string? connectionString)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

        if (connectionString is not null)
        {
            builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = connectionString;
        }

        return builder;
    }

    [Fact]
    public void DatabaseConnectionNameIsJournalSystem()
    {
        // Arrange & Act
        var name = HostApplicationBuilderExtensions.DatabaseConnectionName;

        // Assert
        name.ShouldBe("journalsystem");
        name.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void RegisterOrganizationServicesReturnsTheSameBuilder()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        var result = builder.RegisterOrganizationServices();

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void RegisterOrganizationServicesRegistersDataContextForPostgreSql()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterOrganizationServices();
        using var provider = builder.Services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrganizationDataContext>();

        // Assert
        context.Database.ProviderName.ShouldBe("Npgsql.EntityFrameworkCore.PostgreSQL");
        context.Database.GetConnectionString().ShouldNotBeNull().ShouldContain("Database=journalsystem");
    }

    [Fact]
    public void RegisterOrganizationServicesDoesNotRegisterContextAsSingleton()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        builder.RegisterOrganizationServices();

        // Assert
        var descriptor = builder.Services.Single(d => d.ServiceType == typeof(OrganizationDataContext));
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        descriptor.Lifetime.ShouldNotBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void ResolvingDataContextWithoutConnectionStringThrows()
    {
        // Arrange
        var builder = CreateBuilder(connectionString: null);
        builder.RegisterOrganizationServices();
        using var provider = builder.Services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var action = () => scope.ServiceProvider.GetRequiredService<OrganizationDataContext>();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void RegisterOrganizationServicesWithNullBuilderThrows()
    {
        // Arrange
        HostApplicationBuilder builder = null!;

        // Act
        var action = () => builder.RegisterOrganizationServices();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task InitializeOrganizationDatabaseWithNullServicesThrows()
    {
        // Arrange
        IServiceProvider services = null!;

        // Act
        var action = () => services.InitializeOrganizationDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        await action.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InitializeOrganizationDatabaseWithoutRegisteredContextThrows()
    {
        // Arrange
        await using var provider = new ServiceCollection().BuildServiceProvider();

        // Act
        var action = () => provider.InitializeOrganizationDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        await action.ShouldThrowAsync<InvalidOperationException>();
    }
}
