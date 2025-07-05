namespace CarInsuranceBot.Application.AI;
public interface IOpenAIService
{
    Task<string> AskAsync(long chatId, string prompt, CancellationToken ct);
}