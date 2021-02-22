using System.Net.Http;
using System.Net.Http.Headers;

namespace TomPIT.Sdk.Rest
{
    internal class RestApiClient
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public RestApiClient()
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var stringTask = _httpClient.GetStringAsync("https://api.github.com/orgs/dotnet/repos");
        }
    }
}
