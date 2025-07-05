using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Application.OCR;
using CarInsuranceBot.Domain.Entities;
using System.Text;
using MediatR;

namespace CarInsuranceBot.Application.Commands.Review;

public class ExtractAndReviewCommandHandler : IRequestHandler<ExtractAndReviewCommand, string>
{
    private readonly IUnitOfWork _uow;
    private readonly IMindeeService _ocr;

    public ExtractAndReviewCommandHandler(IUnitOfWork uow, IMindeeService ocr)
    {
        _uow = uow;
        _ocr = ocr;
    }

    public async Task<string> Handle(ExtractAndReviewCommand cmd, CancellationToken ct)
    {
        var doc = await _uow.Documents.GetAsync(cmd.DocumentId, ct)
                  ?? throw new KeyNotFoundException("Document not found");

        await using var fs = File.OpenRead(doc.Path);
        var extracted = await _ocr.ExtractAsync(fs, doc.Type, ct);


        // store each field
        foreach (var (name, value) in extracted.Values)
        {
            _uow.ExtractedFields.Add(new ExtractedField
            {
                DocumentId = doc.Id,
                FieldName = name,
                FieldValue = value,
                Confidence = 0.9f
            });
        }

        await _uow.SaveChangesAsync(ct);   // ← make sure DB commit happens

        var sb = new StringBuilder("🔍 *Please review the extracted data:*");
        foreach (var (n, v) in extracted.Values)
            sb.Append($"\n• *{n}*: `{v}`");

        sb.Append("\n\nType *yes* to continue or *retry* to upload new photos.");
        return sb.ToString();
    }
}