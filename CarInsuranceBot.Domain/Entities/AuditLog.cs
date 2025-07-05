using CarInsuranceBot.Domain.Common;

namespace CarInsuranceBot.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public string TableName { get; set; } = "";
        public Guid RecordId { get; set; }
        public string Action { get; set; } = "";
        public string JsonDiff { get; set; } = "";
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}