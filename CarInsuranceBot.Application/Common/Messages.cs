using CarInsuranceBot.Domain.Enums;

namespace CarInsuranceBot.Application.Common;

public static class Messages
{
    public static string AlreadyInProgress() =>
        "🚧 A request is already in progress.\n" +
        "Type /cancel to abandon it before starting a new one.";

    public static string Intro() => "👋 Welcome to FastCar Insurance Bot!\n" +
        "1️⃣  Send a photo of your **passport** …";

    public static string ResetDone() =>
        "🔄 Your previous attempt looked incomplete so I reset it.\n" +
        "Let's start fresh – please send your passport photo.";

    public static string GreetByStage(RegistrationStage stage) =>
        stage switch
        {
            RegistrationStage.WaitingForPassport  => "Please upload your passport 🛂",
            RegistrationStage.WaitingForVehicle   => "Great! Now send the vehicle registration 📄",
            RegistrationStage.WaitingForReview    => "Type **yes** to confirm the extracted data or **retry** to upload new photos.",
            RegistrationStage.WaitingForPayment   => "Type **yes** to pay 100 USD or **no** to cancel.",
            _                                     => Intro()
        };
} 