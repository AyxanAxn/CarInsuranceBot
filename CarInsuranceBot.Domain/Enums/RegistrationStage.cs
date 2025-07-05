namespace CarInsuranceBot.Domain.Enums;
public enum RegistrationStage
{
    None = 0,
    WaitingForPassport = 1,
    WaitingForVehicle = 2,
    WaitingForReview = 3,
    ReadyToPay = 4,
    WaitingForPayment=5,
    Finished = 6
}
