using ConvenienceStore.Domain.Common;
using FluentValidation;
using MediatR;

namespace ConvenienceStore.Application.Features.System.Queries.Ping;

public record PingQuery(string Message) : IRequest<Result<string>>;

public class PingQueryValidator : AbstractValidator<PingQuery>
{
    public PingQueryValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message cannot be empty.")
            .MaximumLength(50).WithMessage("Message cannot exceed 50 characters.");
    }
}

public class PingQueryHandler : IRequestHandler<PingQuery, Result<string>>
{
    public Task<Result<string>> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success($"Pong: {request.Message} - Server Time: {DateTime.UtcNow:O}"));
    }
}
