namespace CarInsuranceBot.Tests.Helpers;

public static class TestAssertions
{
    public static void ShouldBeSuccessfulPolicyGeneration(this string result)
    {
        result.Should().Be("✅ Policy generated and sent!");
    }

    public static void ShouldBePolicyResent(this string result)
    {
        result.Should().Be("✅ Policy resent.");
    }

    public static void ShouldBeNoPolicyFound(this string result)
    {
        result.Should().Be("❌ No policy found for this chat. Complete the flow first.");
    }

    public static void ShouldBeWelcomeMessage(this string result)
    {
        result.Should().Contain("Welcome");
        result.Should().Contain("Insurance Bot");
    }

    public static void ShouldBePriceQuote(this string result)
    {
        result.Should().Contain("100 USD");
        result.Should().Contain("price");
    }

    public static void ShouldBeCancelledMessage(this string result)
    {
        result.Should().Be("Your session has been cancelled. To start over, type /start or upload your passport.");
    }

    public static void ShouldBeDuplicateDocumentMessage(this string result)
    {
        result.Should().Contain("duplicate");
        result.Should().Contain("different image");
    }

    public static void ShouldBeMaxAttemptsReached(this string result, int maxAttempts = 5)
    {
        result.Should().Contain($"{maxAttempts} upload attempts");
        result.Should().Contain("cancel");
    }

    public static void ShouldBeWaitingForVehicle(this RegistrationStage stage)
    {
        stage.Should().Be(RegistrationStage.WaitingForVehicle);
    }

    public static void ShouldBeWaitingForReview(this RegistrationStage stage)
    {
        stage.Should().Be(RegistrationStage.WaitingForReview);
    }

    public static void ShouldBeFinished(this RegistrationStage stage)
    {
        stage.Should().Be(RegistrationStage.Finished);
    }

    public static void ShouldBeNone(this RegistrationStage stage)
    {
        stage.Should().Be(RegistrationStage.None);
    }
} 