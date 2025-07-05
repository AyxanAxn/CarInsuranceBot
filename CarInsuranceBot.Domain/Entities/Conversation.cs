using CarInsuranceBot.Domain.Common;

namespace CarInsuranceBot.Domain.Entities
{
    public class Conversation : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Prompt { get; set; } = "";
        public string Response { get; set; } = "";
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        // navigation
        public User User { get; set; } = null!;
    }
}