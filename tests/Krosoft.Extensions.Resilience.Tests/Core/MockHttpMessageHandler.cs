using System.Net;

namespace Krosoft.Extensions.Resilience.Tests.Core;

public class MockHttpMessageHandler : DelegatingHandler
{
    private int _count;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                 CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (_count <= 1)
        {
            _count++;
            response.StatusCode = HttpStatusCode.InternalServerError;
        }
        else
        {
            response.StatusCode = HttpStatusCode.OK;
        }

        return response;
    }
}