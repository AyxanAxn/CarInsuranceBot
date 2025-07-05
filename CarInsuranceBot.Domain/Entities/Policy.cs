namespace CarInsuranceBot.Domain.Entities
{
    public class Policy : BaseEntity
    {
        public Guid UserId { get; set; }
        public string PolicyNumber { get; set; } = "";
        public PolicyStatus Status { get; set; } = PolicyStatus.Pending;
        public string PdfPath { get; set; } = "";
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }

        // navigation
        public User User { get; set; } = null!;
    }
}