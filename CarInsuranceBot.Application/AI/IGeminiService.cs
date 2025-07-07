namespace CarInsuranceBot.Application.AI;
public interface IGeminiService
{
    Task<string> AskAsync(long chatId, string prompt, CancellationToken ct);
}