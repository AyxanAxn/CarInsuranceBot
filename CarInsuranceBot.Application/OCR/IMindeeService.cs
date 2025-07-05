using CarInsuranceBot.Domain.Enums;

namespace CarInsuranceBot.Application.OCR;

public interface IMindeeService
{
    Task<ExtractedDocument> ExtractAsync(
        Stream image,
        DocumentType docType,
        CancellationToken ct);
}
