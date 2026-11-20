using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Authentication;

public sealed class EntraSignInSettingsTests
{
    private static IConfiguration Configuration(params (string Key, string? Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(x => new KeyValuePair<string, string?>(x.Key, x.Value)))
            .Build();

    private static (string, string?)[] Complete() =>
    [
        (EntraSignInSettings.EnabledKey, "true"),
        (EntraSignInSettings.TenantIdKey, "tenant-1"),
        (EntraSignInSettings.ClientIdKey, "client-1"),
        (EntraSignInSettings.ClientSecretKey, "secret-1")
    ];

    [Fact]
    public void IsDisabledWhenTheSwitchIsOffOrMissing()
    {
        // Arrange
        var configuration = Configuration((EntraSignInSettings.ClientIdKey, "client-1"));

        // Act
        var settings = EntraSignInSettings.FromConfiguration(configuration);

        // Assert
        settings.Enabled.ShouldBeFalse();
        settings.ShouldBeSameAs(EntraSignInSettings.Disabled);
    }

    [Fact]
    public void BuildsTheEntraAuthorityAndDefaultApiScopeFromTenantAndClient()
    {
        // Arrange
        var configuration = Configuration(Complete());

        // Act
        var settings = EntraSignInSettings.FromConfiguration(configuration);

        // Assert
        settings.Enabled.ShouldBeTrue();
        settings.Authority.ShouldBe(new Uri("https://login.microsoftonline.com/tenant-1/v2.0"));
        settings.ClientId.ShouldBe("client-1");
        settings.ClientSecret.ShouldBe("secret-1");
        settings.ApiScope.ShouldBe("api://client-1/access_as_user");
    }

    [Fact]
    public void AuthorityAndScopeCanBeOverridden()
    {
        // Arrange
        var configuration = Configuration(
        [
            .. Complete(),
            (EntraSignInSettings.AuthorityKey, "http://localhost:9999/tenant/v2.0"),
            (EntraSignInSettings.ApiScopeKey, "api://other/scope")
        ]);

        // Act
        var settings = EntraSignInSettings.FromConfiguration(configuration);

        // Assert
        settings.Authority.ShouldBe(new Uri("http://localhost:9999/tenant/v2.0"));
        settings.ApiScope.ShouldBe("api://other/scope");
    }

    [Fact]
    public void TenantIsNotRequiredWhenTheAuthorityIsOverridden()
    {
        // Arrange
        var configuration = Configuration(
            (EntraSignInSettings.EnabledKey, "true"),
            (EntraSignInSettings.ClientIdKey, "client-1"),
            (EntraSignInSettings.ClientSecretKey, "secret-1"),
            (EntraSignInSettings.AuthorityKey, "http://localhost:9999/tenant/v2.0"));

        // Act
        var settings = EntraSignInSettings.FromConfiguration(configuration);

        // Assert
        settings.Enabled.ShouldBeTrue();
    }

    [Fact]
    public void NamesEveryMissingSettingWhenEnabledButIncomplete()
    {
        // Arrange
        var configuration = Configuration((EntraSignInSettings.EnabledKey, "true"));

        // Act
        var action = () => EntraSignInSettings.FromConfiguration(configuration);

        // Assert
        var message = action.ShouldThrow<InvalidOperationException>().Message;
        message.ShouldContain(EntraSignInSettings.ClientIdKey);
        message.ShouldContain(EntraSignInSettings.ClientSecretKey);
        message.ShouldContain(EntraSignInSettings.TenantIdKey);
    }

    [Fact]
    public void ThrowsForANullConfiguration()
    {
        // Arrange & Act
        var action = () => EntraSignInSettings.FromConfiguration(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
