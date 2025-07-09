namespace CarInsuranceBot.Domain.Entities
{
    public class User : BaseEntity
    {
        public long TelegramUserId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public int UploadAttempts { get; set; }
        // navigation
        public List<Document> Documents { get; set; } = [];
        public List<Policy> Policies { get; set; } = [];
        public List<Conversation> Conversations { get; set; } = [];
        public RegistrationStage Stage { get; set; } = RegistrationStage.None;

        public bool IsInconsistent()
        {
            // Check if user stage doesn't match their data
            return Stage switch
            {
                RegistrationStage.WaitingForVehicle => !Documents.Any(d => d.Type == Domain.Enums.DocumentType.Passport),// Should have passport document
                RegistrationStage.WaitingForReview => !Documents.Any(d => d.Type == Domain.Enums.DocumentType.Passport) ||
                                           !Documents.Any(d => d.Type == Domain.Enums.DocumentType.VehicleRegistration),// Should have both documents
                _ => false,
            };
        }

        public void Reset()
        {
            Stage = RegistrationStage.None;
            UploadAttempts = 0;
            // Note: Documents and other data will be cleared by the handler
        }
    }
}