using CarInsuranceBot.Application.Commands.Start;
using CarInsuranceBot.Application.Common;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;
using CarInsuranceBot.Tests.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace CarInsuranceBot.Tests.Commands;

public class EnsureFreshStartCommandTests : IClassFixture<InMemoryFixture>
{
    private readonly InMemoryFixture _fx;

    public EnsureFreshStartCommandTests(InMemoryFixture fx) => _fx = fx;

    [Fact]
    public async Task Handle_NewUser_CreatesUserAndReturnsIntro()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var auditService = new Mock<IAuditService>().Object;
        var handler = new EnsureFreshStartHandler(uow, auditService);
        var command = new EnsureFreshStartCommand(1001, "John");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("Welcome to FastCar Insurance Bot", result);
        Assert.Contains("passport", result);
        
        var user = await uow.Users.GetAsync(1001, CancellationToken.None);
        Assert.NotNull(user);
        Assert.Equal("John", user!.FirstName);
        Assert.Equal(RegistrationStage.WaitingForPassport, user.Stage);
    }

    [Fact]
    public async Task Handle_UserInReviewStage_ReturnsAlreadyInProgress()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var auditService = new Mock<IAuditService>().Object;
        var handler = new EnsureFreshStartHandler(uow, auditService);
        var user = new User
        {
            TelegramUserId = 1002,
            FirstName = "John",
            Stage = RegistrationStage.WaitingForReview
        };
        uow.Users.Add(user);
        await uow.SaveChangesAsync(CancellationToken.None);

        var command = new EnsureFreshStartCommand(1002, "John");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("already in progress", result);
        Assert.Contains("/cancel", result);
    }

    [Fact]
    public async Task Handle_UserInPaymentStage_ReturnsAlreadyInProgress()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var auditService = new Mock<IAuditService>().Object;
        var handler = new EnsureFreshStartHandler(uow, auditService);
        var user = new User
        {
            TelegramUserId = 1003,
            FirstName = "John",
            Stage = RegistrationStage.WaitingForPayment
        };
        uow.Users.Add(user);
        await uow.SaveChangesAsync(CancellationToken.None);

        var command = new EnsureFreshStartCommand(1003, "John");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("already in progress", result);
        Assert.Contains("/cancel", result);
    }

    [Fact]
    public async Task Handle_UserInInconsistentState_ResetsAndReturnsResetMessage()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var auditService = new Mock<IAuditService>().Object;
        var handler = new EnsureFreshStartHandler(uow, auditService);
        var user = new User
        {
            TelegramUserId = 1004,
            FirstName = "John",
            Stage = RegistrationStage.WaitingForVehicle // Should have passport but doesn't
        };
        uow.Users.Add(user);
        await uow.SaveChangesAsync(CancellationToken.None);

        var command = new EnsureFreshStartCommand(1004, "John");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("incomplete so I reset it", result);
        Assert.Contains("passport photo", result);
        
        var updatedUser = await uow.Users.GetAsync(1004, CancellationToken.None);
        Assert.NotNull(updatedUser);
        Assert.Equal(RegistrationStage.None, updatedUser!.Stage);
        Assert.Equal(0, updatedUser.UploadAttempts);
    }

    [Fact]
    public async Task Handle_UserInWaitingForPassport_ReturnsPassportMessage()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var auditService = new Mock<IAuditService>().Object;
        var handler = new EnsureFreshStartHandler(uow, auditService);
        var user = new User
        {
            TelegramUserId = 1005,
            FirstName = "John",
            Stage = RegistrationStage.WaitingForPassport
        };
        uow.Users.Add(user);
        await uow.SaveChangesAsync(CancellationToken.None);

        var command = new EnsureFreshStartCommand(1005, "John");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("upload your passport", result);
    }

    [Fact]
    public async Task Handle_UserInWaitingForVehicle_ReturnsVehicleMessage()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var auditService = new Mock<IAuditService>().Object;
        var handler = new EnsureFreshStartHandler(uow, auditService);
        var user = new User
        {
            TelegramUserId = 1006,
            FirstName = "John",
            Stage = RegistrationStage.WaitingForVehicle
        };
        // Add passport document to make state consistent
        var passport = new Document
        {
            UserId = user.Id,
            Type = DocumentType.Passport,
            ContentHash = "hash123"
        };
        uow.Users.Add(user);
        uow.Documents.Add(passport);
        await uow.SaveChangesAsync(CancellationToken.None);

        var command = new EnsureFreshStartCommand(1006, "John");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("vehicle registration", result);
    }

    [Fact]
    public async Task Handle_UserWithNullFirstName_UsesDefaultName()
    {
        // Arrange
        var uow = new UnitOfWork(_fx.Db);
        var auditService = new Mock<IAuditService>().Object;
        var handler = new EnsureFreshStartHandler(uow, auditService);
        var command = new EnsureFreshStartCommand(1007, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await uow.Users.GetAsync(1007, CancellationToken.None);
        Assert.NotNull(user);
        Assert.Equal("Friend", user!.FirstName);
    }
} 