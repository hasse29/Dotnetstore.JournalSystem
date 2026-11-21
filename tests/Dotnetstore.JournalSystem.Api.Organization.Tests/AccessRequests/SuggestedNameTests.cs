using Dotnetstore.JournalSystem.Api.Organization.AccessRequests;
using Dotnetstore.JournalSystem.Shared.Constants;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.AccessRequests;

public sealed class SuggestedNameTests
{
    [Theory]
    [InlineData("Anna Svensson", "Anna", "Svensson")]
    [InlineData("Anna Maria Svensson", "Anna Maria", "Svensson")]
    [InlineData("Madonna", "Madonna", "Madonna")]
    [InlineData("   ", "fallback@example.com", "fallback@example.com")]
    public void TakesTheLastWordAsTheSurname(string displayName, string first, string last)
    {
        // Arrange & Act
        var (firstName, lastName) = SuggestedName.Split(displayName, "fallback@example.com");

        // Assert
        firstName.ShouldBe(first);
        lastName.ShouldBe(last);
    }

    [Fact]
    public void KeepsNamesWithinTheColumnLimit()
    {
        // Arrange & Act
        var (firstName, lastName) = SuggestedName.Split(new string('a', 80) + " " + new string('b', 80), "x");

        // Assert
        firstName.Length.ShouldBe(DataSchemeConstants.UserNameMaxLength);
        lastName.Length.ShouldBe(DataSchemeConstants.UserNameMaxLength);
    }
}
