using System.Collections.Generic;
using CarInsuranceBot.Domain.Enums;

namespace CarInsuranceBot.Application.OCR;

public record ExtractedDocument(DocumentType DocType)
{
    public Dictionary<string, string> Values { get; init; } = new();
    public Dictionary<string, string> Fields => Values;
    public ExtractedDocument Add(string name, string value)
    {
        Values[name] = value;
        return this;
    }
}