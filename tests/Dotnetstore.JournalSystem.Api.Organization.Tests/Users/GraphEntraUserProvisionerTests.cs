using Dotnetstore.JournalSystem.Api.Organization.Users;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class GraphEntraUserProvisionerTests
{
    [Fact]
    public void GenerateTemporaryPasswordHasLengthAndAllCharacterClasses()
    {
        // Arrange & Act
        var password = GraphEntraUserProvisioner.GenerateTemporaryPassword();

        // Assert
        password.Length.ShouldBe(20);
        password.Any(char.IsAsciiLetterLower).ShouldBeTrue();
        password.Any(char.IsAsciiLetterUpper).ShouldBeTrue();
        password.Any(char.IsAsciiDigit).ShouldBeTrue();
        password.Any(c => !char.IsAsciiLetterOrDigit(c)).ShouldBeTrue();
    }

    [Fact]
    public void GenerateTemporaryPasswordIsDifferentEachTime()
    {
        // Arrange & Act
        var passwords = Enumerable.Range(0, 20).Select(_ => GraphEntraUserProvisioner.GenerateTemporaryPassword()).ToList();

        // Assert
        passwords.Distinct().Count().ShouldBe(20);
    }

    [Theory]
    [InlineData("anna.svensson@example.com", "annasvensson")]
    [InlineData("a_b-c+d@example.com", "abcd")]
    public void ToMailNicknameKeepsOnlyLettersAndDigitsOfTheLocalPart(string username, string expected)
    {
        // Arrange & Act
        var nickname = GraphEntraUserProvisioner.ToMailNickname(username);

        // Assert
        nickname.ShouldBe(expected);
    }

    [Fact]
    public void ToMailNicknameFallsBackToRandomValueWhenNothingUsableRemains()
    {
        // Arrange & Act
        var nickname = GraphEntraUserProvisioner.ToMailNickname("___@example.com");

        // Assert
        nickname.Length.ShouldBe(12);
        nickname.ShouldAllBe(c => char.IsAsciiLetterOrDigit(c));
    }
}
