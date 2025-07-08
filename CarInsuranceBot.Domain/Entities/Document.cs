namespace CarInsuranceBot.Domain.Entities
{
    public class Document : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Path { get; set; } = "";
        public DocumentType Type { get; set; }
        public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;
        public string? ContentHash { get; set; }                      

        // navigation
        public User User { get; set; } = null!;
        public List<ExtractedField> ExtractedFields { get; set; } = new();
    }
}