using System.Threading;
using System.Threading.Tasks;

namespace CarInsuranceBot.Application.Common.Interfaces;

public interface IPolicyFileStore : IFileStore
{
    Task<string> SavePdf(byte[] pdfBytes, string fileName, CancellationToken ct = default);
}