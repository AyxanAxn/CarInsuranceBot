using CarInsuranceBot.Application.AI;
using CarInsuranceBot.Application.Common.Interfaces;
using MediatR;

namespace CarInsuranceBot.Application.Queries.Chat;

public class ChatQueryHandler : IRequestHandler<ChatQuery, string>
{
    private readonly IGeminiService _ai;
    private readonly IUnitOfWork _uow;

    public ChatQueryHandler(IGeminiService ai, IUnitOfWork uow)
    {
        _ai = ai;
        _uow = uow;
    }

    public async Task<string> Handle(ChatQuery q, CancellationToken ct)
    {
        var answer = await _ai.AskAsync(q.ChatId, q.Prompt, ct);
        return answer;
    }
}
