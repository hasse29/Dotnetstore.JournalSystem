using Dotnetstore.JournalSystem.Host.Extensions;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Auth;

public sealed class EntraConfigurationTests
{
    private static IConfiguration Build(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static Dictionary<string, string?> Complete() => new()
    {
        ["AzureAd:TenantId"] = "d603805b-1934-4b76-9f94-6587640f80bf",
        ["AzureAd:ClientId"] = "53367cbc-d34c-4a13-af71-83ede93d6b24",
        ["AzureAd:ClientCredentials:0:SourceType"] = "ClientSecret",
        ["AzureAd:ClientCredentials:0:ClientSecret"] = "not-a-real-secret"
    };

    [Fact]
    public void CompleteConfigurationIsAccepted()
    {
        // Arrange
        var configuration = Build(Complete());

        // Act
        var act = () => AuthenticationExtensions.ValidateEntraConfiguration(configuration);

        // Assert
        Should.NotThrow(act);
    }

    [Theory]
    [InlineData("AzureAd:TenantId")]
    [InlineData("AzureAd:ClientId")]
    public void MissingTenantOrClientIdIsRejectedAndNamed(string key)
    {
        // Arrange
        var values = Complete();
        values[key] = "";
        var configuration = Build(values);

        // Act
        var act = () => AuthenticationExtensions.ValidateEntraConfiguration(configuration);

        // Assert
        Should.Throw<InvalidOperationException>(act).Message.ShouldContain(key);
    }

    [Fact]
    public void MissingClientCredentialsAreRejectedAndNamed()
    {
        // Arrange
        var values = Complete()
            .Where(x => !x.Key.StartsWith("AzureAd:ClientCredentials", StringComparison.Ordinal))
            .ToDictionary(x => x.Key, x => x.Value);
        var configuration = Build(values);

        // Act
        var act = () => AuthenticationExtensions.ValidateEntraConfiguration(configuration);

        // Assert
        Should.Throw<InvalidOperationException>(act).Message.ShouldContain("ClientCredentials");
    }

    [Fact]
    public async Task HostStartsWithEntraEnabledWhenConfigurationIsComplete()
    {
        // Arrange
        var settings = Complete().ToDictionary(x => x.Key, x => x.Value);
        settings["Authentication:Entra:Enabled"] = "true";
        await using var factory = TestHost.Create("Production", settings);
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public void HostRefusesToStartWithEntraEnabledAndNoCredentials()
    {
        // Arrange
        var settings = new Dictionary<string, string?> { ["Authentication:Entra:Enabled"] = "true" };
        using var factory = TestHost.Create("Production", settings);

        // Act
        var act = () => factory.CreateClient();

        // Assert
        Should.Throw<InvalidOperationException>(act).Message.ShouldContain("ClientCredentials");
    }
}
