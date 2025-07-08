using CarInsuranceBot.Application.Commands.Retry;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;
using CarInsuranceBot.Tests.Helpers;

namespace CarInsuranceBot.Tests.Commands;

public class RetryCommandTests : IClassFixture<InMemoryFixture>
{
    private readonly InMemoryFixture _fx;

    public RetryCommandTests(InMemoryFixture fx) => _fx = fx;

    [Fact]
    public async Task Handle_UserInReviewStage_ResetsAndReturnsSuccessMessage()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var handler = new RetryCommandHandler(uow);
        var user = new User
        {
            TelegramUserId = 1001,
            FirstName = "John",
            Stage = RegistrationStage.WaitingForReview
        };
        uow.Users.Add(user);
        await uow.SaveChangesAsync(CancellationToken.None);

        var command = new RetryCommand(1001);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("try again", result);
        Assert.Contains("passport", result);
        
        var updatedUser = await uow.Users.GetAsync(1001, CancellationToken.None);
        Assert.NotNull(updatedUser);
        Assert.Equal(RegistrationStage.WaitingForPassport, updatedUser!.Stage);
        Assert.Equal(0, updatedUser.UploadAttempts);
    }

    [Fact]
    public async Task Handle_UserNotInReviewStage_ReturnsError()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var handler = new RetryCommandHandler(uow);
        var user = new User
        {
            TelegramUserId = 1002,
            FirstName = "John",
            Stage = RegistrationStage.WaitingForPassport
        };
        uow.Users.Add(user);
        await uow.SaveChangesAsync(CancellationToken.None);

        var command = new RetryCommand(1002);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("only retry when reviewing", result);
        
        var updatedUser = await uow.Users.GetAsync(1002, CancellationToken.None);
        Assert.NotNull(updatedUser);
        Assert.Equal(RegistrationStage.WaitingForPassport, updatedUser!.Stage); // Should not change
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsError()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var handler = new RetryCommandHandler(uow);
        var command = new RetryCommand(9999);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("User not found", result);
        Assert.Contains("/start", result);
    }
} 