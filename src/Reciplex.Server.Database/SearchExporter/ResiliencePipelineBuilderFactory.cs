using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Reciplex.Server.Database.SearchExporter.Loggers;

namespace Reciplex.Server.Database.SearchExporter;

class ResiliencePipelineBuilderFactory(ILogger<ResiliencePipelineBuilderFactory> logger)
{
    public ResiliencePipeline BuildDeleteRowPipeline(Func<Exception, bool> shouldHandle)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(
                new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle(shouldHandle),
                    MaxRetryAttempts = 5,
                    Delay = TimeSpan.FromMilliseconds(20),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    OnRetry = args =>
                    {
                        logger.Warning_DeleteAttemptPlanned(
                            args.AttemptNumber + 1,
                            args.Outcome.Exception
                        );
                        return ValueTask.CompletedTask;
                    },
                }
            )
            .Build();
    }
}
