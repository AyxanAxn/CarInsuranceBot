namespace CarInsuranceBot.Domain.Entities
{
    public class ErrorLog : BaseEntity
    {
        public string Message { get; set; } = "";
        public string StackTrace { get; set; } = "";
        public DateTime LoggedUtc { get; set; } = DateTime.UtcNow;
    }
}
