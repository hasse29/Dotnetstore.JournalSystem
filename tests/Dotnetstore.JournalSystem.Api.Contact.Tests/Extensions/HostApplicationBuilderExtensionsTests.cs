using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Extensions;

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
    public void RegisterContactServicesReturnsTheSameBuilder()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        var result = builder.RegisterContactServices();

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void RegisterContactServicesRegistersDataContextForSqlServer()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContactDataContext>();

        // Assert
        context.Database.ProviderName.ShouldBe("Microsoft.EntityFrameworkCore.SqlServer");
        context.Database.GetConnectionString().ShouldNotBeNull().ShouldContain("Initial Catalog=journalsystem");
    }

    [Fact]
    public void RegisterContactServicesDoesNotRegisterContextAsSingleton()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        builder.RegisterContactServices();

        // Assert
        var descriptor = builder.Services.Single(d => d.ServiceType == typeof(ContactDataContext));
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        descriptor.Lifetime.ShouldNotBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void ResolvingDataContextWithoutConnectionStringThrows()
    {
        // Arrange
        var builder = CreateBuilder(connectionString: null);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var action = () => scope.ServiceProvider.GetRequiredService<ContactDataContext>();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void RegisterContactServicesWithNullBuilderThrows()
    {
        // Arrange
        HostApplicationBuilder builder = null!;

        // Act
        var action = () => builder.RegisterContactServices();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task InitializeContactDatabaseWithNullServicesThrows()
    {
        // Arrange
        IServiceProvider services = null!;

        // Act
        var action = () => services.InitializeContactDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        await action.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InitializeContactDatabaseWithoutRegisteredContextThrows()
    {
        // Arrange
        await using var provider = new ServiceCollection().BuildServiceProvider();

        // Act
        var action = () => provider.InitializeContactDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        await action.ShouldThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void RegisterContactServicesRegistersAddressTypeServicesAsScoped()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        builder.RegisterContactServices();

        // Assert
        foreach (var serviceType in new[] { typeof(IAddressTypeRepository), typeof(IAddressTypeService) })
        {
            builder.Services.Single(d => d.ServiceType == serviceType).Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
    }

    [Fact]
    public void RegisterContactServicesMakesAddressTypeServiceResolvable()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Act
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<IAddressTypeService>();

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void ResolvingAddressTypeServiceFromRootProviderIsRejected()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        // Act
        var action = () => provider.GetService<IAddressTypeService>();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void RegisterContactServicesRegistersPhoneNumberTypeServicesAsScoped()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        builder.RegisterContactServices();

        // Assert
        foreach (var serviceType in new[] { typeof(IPhoneNumberTypeRepository), typeof(IPhoneNumberTypeService) })
        {
            builder.Services.Single(d => d.ServiceType == serviceType).Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
    }

    [Fact]
    public void RegisterContactServicesMakesPhoneNumberTypeServiceResolvable()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Act
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<IPhoneNumberTypeService>();

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void ResolvingPhoneNumberTypeServiceFromRootProviderIsRejected()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        // Act
        var action = () => provider.GetService<IPhoneNumberTypeService>();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void RegisterContactServicesRegistersEmailAddressTypeServicesAsScoped()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        builder.RegisterContactServices();

        // Assert
        foreach (var serviceType in new[] { typeof(IEmailAddressTypeRepository), typeof(IEmailAddressTypeService) })
        {
            builder.Services.Single(d => d.ServiceType == serviceType).Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
    }

    [Fact]
    public void RegisterContactServicesMakesEmailAddressTypeServiceResolvable()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Act
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<IEmailAddressTypeService>();

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void ResolvingEmailAddressTypeServiceFromRootProviderIsRejected()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        // Act
        var action = () => provider.GetService<IEmailAddressTypeService>();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void RegisterContactServicesRegistersEmailAddressServicesAsScoped()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        builder.RegisterContactServices();

        // Assert
        foreach (var serviceType in new[] { typeof(IEmailAddressRepository), typeof(IEmailAddressService) })
        {
            builder.Services.Single(d => d.ServiceType == serviceType).Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
    }

    [Fact]
    public void RegisterContactServicesMakesEmailAddressServiceResolvable()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Act
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<IEmailAddressService>();

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void ResolvingEmailAddressServiceFromRootProviderIsRejected()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        // Act
        var action = () => provider.GetService<IEmailAddressService>();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void RegisterContactServicesRegistersPhoneNumberServicesAsScoped()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        builder.RegisterContactServices();

        // Assert
        foreach (var serviceType in new[] { typeof(IPhoneNumberRepository), typeof(IPhoneNumberService) })
        {
            builder.Services.Single(d => d.ServiceType == serviceType).Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
    }

    [Fact]
    public void RegisterContactServicesMakesPhoneNumberServiceResolvable()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Act
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<IPhoneNumberService>();

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void ResolvingPhoneNumberServiceFromRootProviderIsRejected()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        // Act
        var action = () => provider.GetService<IPhoneNumberService>();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void RegisterContactServicesRegistersAddressServicesAsScoped()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);

        // Act
        builder.RegisterContactServices();

        // Assert
        foreach (var serviceType in new[] { typeof(IAddressRepository), typeof(IAddressService) })
        {
            builder.Services.Single(d => d.ServiceType == serviceType).Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
    }

    [Fact]
    public void RegisterContactServicesMakesAddressServiceResolvable()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // Act
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<IAddressService>();

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void ResolvingAddressServiceFromRootProviderIsRejected()
    {
        // Arrange
        var builder = CreateBuilder(ConnectionString);
        builder.RegisterContactServices();
        using var provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        // Act
        var action = () => provider.GetService<IAddressService>();

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void MapContactEndpointsReturnsTheSameBuilder()
    {
        // Arrange
        using var app = WebApplication.CreateBuilder().Build();

        // Act
        var result = app.MapContactEndpoints();

        // Assert
        result.ShouldBeSameAs(app);
    }

    [Fact]
    public void MapContactEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapContactEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
