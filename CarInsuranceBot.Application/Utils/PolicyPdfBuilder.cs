using QuestPDF.Fluent;

namespace CarInsuranceBot.Application.Utils;

public static class PolicyPdfBuilder
{
    public static byte[] Build(string fullName, string vin, DateTime expiry)
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
                    col.Item().Text($"Policy Holder: {fullName}");
                    col.Item().Text($"Vehicle VIN: {vin}");
                    col.Item().Text($"Price Paid: 100 USD");
                    col.Item().Text($"Valid Until: {expiry:yyyy-MM-dd}");
                    col.Item().PaddingTop(20).Text("This is a dummy policy for demo purposes only.");
                });
            });
        }).GeneratePdf(ms);
        return ms.ToArray();
    }
} 