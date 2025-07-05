using MediatR;

namespace CarInsuranceBot.Application.Queries.Chat;

public record ChatQuery(long ChatId, string Prompt) : IRequest<string>;
