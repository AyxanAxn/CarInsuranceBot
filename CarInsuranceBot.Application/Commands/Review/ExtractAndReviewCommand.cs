using MediatR;

namespace CarInsuranceBot.Application.Commands.Review;

public record ExtractAndReviewCommand(Guid DocumentId) : IRequest<string>;
