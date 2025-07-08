using QuestPDF.Fluent;
using System;
using System.IO;
using System.Collections.Generic;

namespace CarInsuranceBot.Application.Utils;

public class PolicyPdfBuilder
{
    private string _fullName = "";
    private string _vin = "";
    private DateTime _expiry = DateTime.UtcNow;
    private readonly List<string> _extraSections = new();

    public PolicyPdfBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    public PolicyPdfBuilder WithVin(string vin)
    {
        _vin = vin;
        return this;
    }

    public PolicyPdfBuilder WithExpiry(DateTime expiry)
    {
        _expiry = expiry;
        return this;
    }

    public PolicyPdfBuilder AddSection(string text)
    {
        _extraSections.Add(text);
        return this;
    }

    public byte[] Build()
    {
        using var ms = new MemoryStream();
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Content().Column(col =>
                {
                    col.Item().Text("FastCar Insurance").FontSize(24).Bold();
                    col.Item().Text($"Policy Holder: {_fullName}");
                    col.Item().Text($"Vehicle VIN: {_vin}");
                    col.Item().Text($"Price Paid: 100 USD");
                    col.Item().Text($"Valid Until: {_expiry:yyyy-MM-dd}");
                    foreach (var section in _extraSections)
                        col.Item().Text(section);
                    col.Item().PaddingTop(20).Text("This is a dummy policy for demo purposes only.");
                });
            });
        }).GeneratePdf(ms);
        return ms.ToArray();
    }
} 