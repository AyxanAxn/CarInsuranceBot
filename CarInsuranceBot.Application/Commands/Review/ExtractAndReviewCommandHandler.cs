using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Application.OCR;
using CarInsuranceBot.Domain.Entities;
using CarInsuranceBot.Domain.Enums;
using System.Text;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Review;

public class ExtractAndReviewCommandHandler(IUnitOfWork uow, IMindeeService ocr, IFileStore fileStore) : IRequestHandler<ExtractAndReviewCommand, string>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IMindeeService _ocr = ocr;
    private readonly IFileStore _fileStore = fileStore;
    public async Task<string> Handle(ExtractAndReviewCommand cmd, CancellationToken ct)
    {
        var doc = await _uow.Documents.GetAsync(cmd.DocumentId, ct)
                  ?? throw new KeyNotFoundException("Document not found");

        // Get all extracted fields for this user (both passport and vehicle registration)
        var userDocuments = await _uow.Documents.GetByUserAsync(doc.UserId, ct);
        var allExtractedFields = new List<ExtractedField>();
        
        foreach (var userDoc in userDocuments)
        {
            var fields = await _uow.ExtractedFields.GetByDocumentAsync(userDoc.Id, ct);
            allExtractedFields.AddRange(fields);
        }

        var sb = new StringBuilder("🔍 *Please review the extracted data:*\n\n");
        
        // Group fields by document type for better presentation
        var passportFields = allExtractedFields.Where(f => f.Document.Type == DocumentType.Passport).ToList();
        var vehicleFields = allExtractedFields.Where(f => f.Document.Type == DocumentType.VehicleRegistration).ToList();

        if (passportFields.Any())
        {
            sb.AppendLine("*📄 Passport Data:*");
            foreach (var field in passportFields)
                sb.AppendLine($"• *{field.FieldName}*: `{field.FieldValue}`");
            sb.AppendLine();
        }

        if (vehicleFields.Any())
        {
            sb.AppendLine("*🚗 Vehicle Registration Data:*");
            foreach (var field in vehicleFields)
                sb.AppendLine($"• *{field.FieldName}*: `{field.FieldValue}`");
            sb.AppendLine();
        }

        sb.Append("Type *yes* to continue or *retry* to upload new photos.");
        return sb.ToString();
    }

    private static string GetBlobName(string pathOrUrl)
    {
        if (!pathOrUrl.Contains('/'))
            return pathOrUrl;

        if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var uri))
            return uri.Segments.Last();

        return pathOrUrl.Split('/').Last();
    }
}