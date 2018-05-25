using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NetCrawler.Tests.Helpers
{
    public abstract class MockHttpHandler : HttpClientHandler
    {
        public HttpRequestMessage RequestMessage { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestMessage = request;
            return await SendAsync(request);
        }

        public abstract Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
