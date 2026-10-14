using MediatR;
using Microsoft.Extensions.Logging;

namespace ConvenienceStore.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("[START] Processing request {RequestName} {@Request}", requestName, request);

        try
        {
            var response = await next();
            _logger.LogInformation("[COMPLETED] Request {RequestName} handled successfully", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ERROR] Request {RequestName} failed with message: {Message}", requestName, ex.Message);
            throw;
        }
    }
}
