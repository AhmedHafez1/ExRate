using System.Net;

public class MockHttpMessageHandler : HttpMessageHandler
{
    public Func<HttpRequestMessage, HttpResponseMessage> SendFunction { get; set; } =
        _ => new HttpResponseMessage(HttpStatusCode.OK);

    public Func<
        HttpRequestMessage,
        CancellationToken,
        HttpResponseMessage
    > SendAsyncFunction { get; set; } =
        (request, token) => new HttpResponseMessage(HttpStatusCode.OK);

    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        return SendFunction(request);
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(SendAsyncFunction(request, cancellationToken));
    }
}
