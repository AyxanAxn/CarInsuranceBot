using CarInsuranceBot.Domain.Common;

namespace CarInsuranceBot.Domain.Entities
{

    public class ExtractedField : BaseEntity
    {
        public Guid DocumentId { get; set; }
        public string FieldName { get; set; } = "";
        public string FieldValue { get; set; } = "";
        public float Confidence { get; set; }

        // navigation
        public Document Document { get; set; } = null!;
    }
}
