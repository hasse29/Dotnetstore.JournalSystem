using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Extensions;

public sealed class HostApplicationBuilderExtensionsTests
{
    private const string ConnectionString = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;Connect Timeout=1;TrustServerCertificate=True";

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
    public void RegisterOrganizationServicesRegistersDataContextForSqlServer()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterOrganizationServices();
        using var provider = builder.Services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrganizationDataContext>();

        // Assert
        context.Database.ProviderName.ShouldBe("Microsoft.EntityFrameworkCore.SqlServer");
        context.Database.GetConnectionString().ShouldNotBeNull().ShouldContain("Initial Catalog=journalsystem");
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

    [Fact]
    public void RegisterOrganizationServicesRegistersOwnCompanyServicesAsScoped()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        builder.RegisterOrganizationServices();

        // Assert
        foreach (var serviceType in new[] { typeof(IOwnCompanyRepository), typeof(IOrganizationUnitOfWork), typeof(IOwnCompanyService) })
        {
            builder.Services.Single(d => d.ServiceType == serviceType).Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
    }

    [Fact]
    public void RegisterOrganizationServicesMakesOwnCompanyServiceResolvable()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterOrganizationServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Act
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<IOwnCompanyService>();

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void ResolvingOwnCompanyServiceFromRootProviderIsRejected()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterOrganizationServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        // Act
        var action = () => provider.GetService<IOwnCompanyService>();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void RegisterOrganizationServicesRegistersRoleServicesAsScoped()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        builder.RegisterOrganizationServices();

        // Assert
        foreach (var serviceType in new[] { typeof(IRoleRepository), typeof(IRoleService) })
        {
            builder.Services.Single(d => d.ServiceType == serviceType).Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
    }

    [Fact]
    public void RegisterOrganizationServicesMakesRoleServiceResolvable()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterOrganizationServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Act
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<IRoleService>();

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void ResolvingRoleServiceFromRootProviderIsRejected()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterOrganizationServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        // Act
        var action = () => provider.GetService<IRoleService>();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void MapOrganizationEndpointsReturnsTheSameBuilder()
    {
        // Arrange
        using var app = WebApplication.CreateBuilder().Build();

        // Act
        var result = app.MapOrganizationEndpoints();

        // Assert
        result.ShouldBeSameAs(app);
    }

    [Fact]
    public void MapOrganizationEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapOrganizationEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
