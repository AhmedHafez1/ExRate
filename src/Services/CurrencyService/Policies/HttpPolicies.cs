using System.Net;
using Polly;
using Polly.Retry;

namespace CurrencyService.Policies;

public static class HttpPolicies
{
    public static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy(
        WebApplicationBuilder builder
    )
    {
        var loggerFactory = builder
            .Services.BuildServiceProvider()
            .GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("RetryPolicy");

        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response =>
                (int)response.StatusCode >= 500
                || response.StatusCode == HttpStatusCode.TooManyRequests
            )
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: (retryAttempt, context) =>
                {
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                },
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    if (outcome.Exception != null)
                    {
                        logger.LogWarning(
                            $"Retry attempt {retryAttempt} failed due to exception: {outcome.Exception.Message}. "
                                + $"Retrying after {timespan.TotalSeconds} seconds."
                        );
                    }
                    else
                    {
                        logger.LogWarning(
                            $"Retry attempt {retryAttempt} failed with status code {(int)outcome.Result.StatusCode} ({outcome.Result.StatusCode}). "
                                + $"Retrying after {timespan.TotalSeconds} seconds."
                        );
                    }
                }
            );
    }
}
