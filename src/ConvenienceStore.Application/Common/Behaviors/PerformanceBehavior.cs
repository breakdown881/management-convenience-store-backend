using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ConvenienceStore.Application.Common.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private const int SlowThresholdMs = 500; // Ngưỡng cảnh báo cho POS Retail

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _timer = new Stopwatch();
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        _timer.Restart();

        var response = await next();

        _timer.Stop();
        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > SlowThresholdMs)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogWarning("[SLOW REQUEST] {RequestName} took {ElapsedMilliseconds} ms to execute!", requestName, elapsedMilliseconds);
        }

        return response;
    }
}
