using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetCrawler.Boundaries
{
    public class RestClient : IRestClient
    {
        private readonly HttpClient _httpClient;

        public RestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(5); //Ideally i'd get this from config, injected in
        }

        public async Task<string> GetAsync(Uri uri)
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Get,
            };

            var result = await ExecuteCall(httpRequestMessage);

            return await result.Content.ReadAsStringAsync();
        }

        private async Task<HttpResponseMessage> ExecuteCall(HttpRequestMessage request)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (OperationCanceledException opcEx)
            {
                throw new RestRequestTimedOutException();
            }
            catch (Exception e)
            {
                throw e;
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                throw new RestRequestFailedException();
            }

            return response;
        }
    }
}
