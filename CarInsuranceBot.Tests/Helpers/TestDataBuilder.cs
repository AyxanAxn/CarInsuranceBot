namespace CarInsuranceBot.Tests.Helpers;

public static class TestDataBuilder
{
    public static User CreateUser(long telegramId = 12345, string firstName = "John", string lastName = "Doe")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            TelegramUserId = telegramId,
            FirstName = firstName,
            LastName = lastName,
            Stage = RegistrationStage.None,
            UploadAttempts = 0,
            CreatedUtc = DateTime.UtcNow
        };
    }

    public static Policy CreatePolicy(Guid userId, string policyNumber = "POL-TEST")
    {
        return new Policy
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PolicyNumber = policyNumber,
            Status = PolicyStatus.Issued,
            PdfPath = $"policies/{policyNumber}.pdf",
            IssuedUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresUtc = DateTime.UtcNow.AddDays(364)
        };
    }

    public static Document CreateDocument(Guid userId, DocumentType type = DocumentType.Passport)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Path = $"documents/{Guid.NewGuid()}.pdf",
            UploadedUtc = DateTime.UtcNow,
            ContentHash = Guid.NewGuid().ToString()
        };
    }

    public static ExtractedField CreateExtractedField(Guid documentId, string fieldName = "FullName", string fieldValue = "John Doe")
    {
        return new ExtractedField
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            FieldName = fieldName,
            FieldValue = fieldValue,
        };
    }

    public static ErrorLog CreateErrorLog(string message = "Test error")
    {
        return new ErrorLog
        {
            Id = Guid.NewGuid(),
            Message = message,
            StackTrace = "Test stack trace",
            LoggedUtc = DateTime.UtcNow
        };
    }

    public static Conversation CreateConversation(Guid userId, string prompt = "Test prompt", string response = "Test response")
    {
        return new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Prompt = prompt,
            Response = response,
            CreatedUtc = DateTime.UtcNow
        };
    }
} 