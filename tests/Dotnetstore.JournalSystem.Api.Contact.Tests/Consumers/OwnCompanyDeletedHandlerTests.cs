using Dotnetstore.JournalSystem.Api.Contact.Consumers;
using Dotnetstore.JournalSystem.Shared.Contracts.Events;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Consumers;

public sealed class OwnCompanyDeletedHandlerTests
{
    private sealed class RecordingCleanup : IOwnCompanyContactCleanup
    {
        public List<Guid> Cleaned { get; } = [];

        public Task DeleteForCompanyAsync(Guid ownCompanyId, CancellationToken cancellationToken = default)
        {
            Cleaned.Add(ownCompanyId);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task HandleAsyncCleansUpTheDeletedCompany()
    {
        // Arrange
        var cleanup = new RecordingCleanup();
        var message = new OwnCompanyDeleted(Guid.NewGuid());

        // Act
        await OwnCompanyDeletedHandler.HandleAsync(message, cleanup, TestContext.Current.CancellationToken);

        // Assert
        cleanup.Cleaned.ShouldBe([message.OwnCompanyId]);
    }

    [Fact]
    public async Task HandleAsyncRejectsAMissingMessageWithoutCleaningUp()
    {
        // Arrange
        var cleanup = new RecordingCleanup();

        // Act
        var action = () => OwnCompanyDeletedHandler.HandleAsync(null!, cleanup, TestContext.Current.CancellationToken);

        // Assert
        await action.ShouldThrowAsync<ArgumentNullException>();
        cleanup.Cleaned.ShouldBeEmpty();
    }
}
