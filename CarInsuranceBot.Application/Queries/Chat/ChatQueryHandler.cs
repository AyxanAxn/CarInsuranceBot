using System.Threading;
using System.Threading.Tasks;
using CarInsuranceBot.Application.AI;
using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Domain.Entities;
using MediatR;

namespace CarInsuranceBot.Application.Queries.Chat;

public class ChatQueryHandler : IRequestHandler<ChatQuery, string>
{
    private readonly IOpenAIService _ai;
    private readonly IUnitOfWork _uow;

    public ChatQueryHandler(IOpenAIService ai, IUnitOfWork uow)
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
