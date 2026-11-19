using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Extensions;

public sealed class ContactDatabaseInitializationTests(SqlServerFixture fixture)
{
    private static ServiceProvider CreateProvider(string connectionString)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = connectionString;
        builder.RegisterContactServices();
        return builder.Services.BuildServiceProvider();
    }

    private static async Task DeleteDatabaseAsync(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ContactDataContext>().Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task InitializeContactDatabaseCreatesTheAddressTypeTableInAnEmptyDatabase()
    {
        // Arrange
        await using var provider = CreateProvider(fixture.CreateDatabaseConnectionString());

        // Act
        await provider.InitializeContactDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContactDataContext>();
        (await context.AddressTypes.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
        await DeleteDatabaseAsync(provider);
    }

    [Fact]
    public async Task InitializeContactDatabaseCreatesItsTablesWhenAnotherModuleAlreadyCreatedTables()
    {
        // Arrange
        var connectionString = fixture.CreateDatabaseConnectionString();
        await using (var other = new OtherModuleContext(connectionString))
        {
            await other.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        await using var provider = CreateProvider(connectionString);

        // Act
        await provider.InitializeContactDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContactDataContext>();
        (await context.AddressTypes.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
        await DeleteDatabaseAsync(provider);
    }

    [Fact]
    public async Task InitializeContactDatabaseCanBeCalledMoreThanOnce()
    {
        // Arrange
        await using var provider = CreateProvider(fixture.CreateDatabaseConnectionString());
        await provider.InitializeContactDatabaseAsync(TestContext.Current.CancellationToken);

        // Act
        var action = () => provider.InitializeContactDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        await Should.NotThrowAsync(action);
        await DeleteDatabaseAsync(provider);
    }

    [Fact]
    public async Task InitializeContactDatabaseFailsForAnUnreachableDatabase()
    {
        // Arrange
        await using var provider = CreateProvider("Server=localhost,1;Database=journalsystem;User Id=u;Password=p;Connect Timeout=1;TrustServerCertificate=True");

        // Act
        var action = () => provider.InitializeContactDatabaseAsync(TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<Exception>(action);
    }

    private sealed class OtherModuleContext(string connectionString) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlServer(connectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<OtherEntity>().ToTable("Other", "OtherModule").HasKey(x => x.Id);
    }

    private sealed class OtherEntity
    {
        public Guid Id { get; set; }
    }
}
