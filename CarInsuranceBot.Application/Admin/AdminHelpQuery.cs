using MediatR;

namespace CarInsuranceBot.Application.Admin;

public record AdminHelpQuery(long ChatId) : IRequest<string>; 