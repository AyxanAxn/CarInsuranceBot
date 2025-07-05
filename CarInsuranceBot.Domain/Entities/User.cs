namespace CarInsuranceBot.Domain.Entities
{
    public class User : BaseEntity
    {
        public long TelegramUserId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public string FullName => $"{FirstName} {LastName}".Trim();

        // navigation
        public List<Document> Documents { get; set; } = [];
        public List<Policy> Policies { get; set; } = [];
        public List<Conversation> Conversations { get; set; } = [];
        public RegistrationStage Stage { get; set; } = RegistrationStage.None;

    }
}
