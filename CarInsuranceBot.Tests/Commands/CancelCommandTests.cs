using CarInsuranceBot.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace CarInsuranceBot.Tests.Commands;

public class CancelCommandTests : IClassFixture<InMemoryFixture>
{
    private readonly ApplicationDbContext _db;
    public CancelCommandTests(InMemoryFixture fx) => _db = fx.Db;

    private class DummyFileStore : IFileStore
    {
        public Task<string> SaveAsync(Telegram.Bot.Types.TGFile telegramFile, CancellationToken ct) => Task.FromResult("");
        public Task<string> SavePdf(byte[] pdfBytes, string fileName, CancellationToken ct = default) => Task.FromResult("");
        public Task<Stream> OpenReadAsync(string path, CancellationToken ct = default) => Task.FromResult<Stream>(new MemoryStream());
        public Task DeleteAsync(string path, CancellationToken ct = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task Cancels_user_registration_and_clears_data()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(123);
        user.Stage = RegistrationStage.WaitingForVehicle;
        
        var document = TestDataBuilder.CreateDocument(user.Id, DocumentType.Passport);
        var extractedField = TestDataBuilder.CreateExtractedField(document.Id);

        _db.Users.Add(user);
        _db.Documents.Add(document);
        _db.ExtractedFields.Add(extractedField);
        await _db.SaveChangesAsync();

        var handler = new CancelCommandHandler(new UnitOfWork(_db), new DummyFileStore());

        // Act
        var result = await handler.Handle(new CancelCommand(user.TelegramUserId), default);

        // Assert
        result.ShouldBeCancelledMessage();

        // Verify user stage is reset
        var updatedUser = await _db.Users.FindAsync(user.Id);
        updatedUser!.Stage.ShouldBeNone();

        // Verify in-progress data is cleared
        _db.Documents.Where(d => d.UserId == user.Id).Should().BeEmpty();
        _db.ExtractedFields.Where(ef => ef.DocumentId == document.Id).Should().BeEmpty();
    }

    [Fact]
    public async Task Cancels_user_with_no_data_returns_success()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser(456);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var handler = new CancelCommandHandler(new UnitOfWork(_db), new DummyFileStore());

        // Act
        var result = await handler.Handle(new CancelCommand(user.TelegramUserId), default);

        // Assert
        result.ShouldBeCancelledMessage();
    }

    [Fact]
    public async Task Cancels_nonexistent_user_returns_success()
    {
        // Arrange
        var handler = new CancelCommandHandler(new UnitOfWork(_db), new DummyFileStore());

        // Act
        var result = await handler.Handle(new CancelCommand(999999), default);

        // Assert
        result.ShouldBeCancelledMessage();
    }
} 