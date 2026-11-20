using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Authentication;

public sealed class EntraFailureCodeTests
{
    [Fact]
    public void FindsTheAadCodeInTheErrorDescription()
    {
        // Arrange & Act
        var code = EntraFailureCode.Extract(null, "invalid_request", "AADSTS50011: The redirect URI does not match.");

        // Assert
        code.ShouldBe("AADSTS50011");
    }

    [Fact]
    public void FindsTheAadCodeInsideAnInnerExceptionMessage()
    {
        // Arrange
        var failure = new InvalidOperationException("outer", new InvalidOperationException("Message contains error: 'invalid_client', error_description: 'AADSTS7000215: Invalid client secret.'"));

        // Act
        var code = EntraFailureCode.Extract(failure, null, null);

        // Assert
        code.ShouldBe("AADSTS7000215");
    }

    [Fact]
    public void FallsBackToTheOAuthErrorWhenThereIsNoAadCode()
    {
        // Arrange & Act
        var code = EntraFailureCode.Extract(new InvalidOperationException("boom"), "access_denied", null);

        // Assert
        code.ShouldBe("access_denied");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("has spaces")]
    [InlineData("UPPERCASE")]
    public void ReturnsNothingWhenThereIsNothingSafeToShow(string? error)
    {
        // Arrange & Act
        var code = EntraFailureCode.Extract(new InvalidOperationException("boom"), error, null);

        // Assert
        code.ShouldBeNull();
    }

    [Theory]
    [InlineData("AADSTS50011", true)]
    [InlineData("access_denied", true)]
    [InlineData("AADSTS", false)]
    [InlineData("<b>x</b>", false)]
    [InlineData(null, false)]
    public void OnlyShortCodesMayBeShownOnThePage(string? code, bool expected)
    {
        // Arrange & Act
        var displayable = EntraFailureCode.IsDisplayable(code);

        // Assert
        displayable.ShouldBe(expected);
    }
}
