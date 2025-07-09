namespace CarInsuranceBot.Domain.Entities.Builders
{
    public class DocumentBuilder
    {
        private readonly Document _doc = new();

        public DocumentBuilder WithUserId(Guid userId)
        {
            _doc.UserId = userId;
            return this;
        }

        public DocumentBuilder WithPath(string path)
        {
            _doc.Path = path;
            return this;
        }

        public DocumentBuilder WithType(DocumentType type)
        {
            _doc.Type = type;
            return this;
        }

        public DocumentBuilder WithUploadedUtc(DateTime uploadedUtc)
        {
            _doc.UploadedUtc = uploadedUtc;
            return this;
        }

        public DocumentBuilder WithContentHash(string? hash)
        {
            _doc.ContentHash = hash;
            return this;
        }

        public DocumentBuilder WithUser(User user)
        {
            _doc.User = user;
            return this;
        }

        public DocumentBuilder WithExtractedFields(List<ExtractedField> fields)
        {
            _doc.ExtractedFields = fields;
            return this;
        }

        public Document Build() => _doc;
    }
} 