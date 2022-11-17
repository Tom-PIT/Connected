using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Connected.SaaS.Clients.Rest
{
    public abstract class EndpointDefinitionsBase
    {
        private readonly IRestClient _restClient;

        public EndpointDefinitionsBase(IRestClient restClient)
        {
            _restClient = restClient;
        }

        private string GetEndpointUrl(string endpoint)
        {
            return $"{this._restClient.EndpointUrl}/{endpoint}";
        }

        protected async Task<TResponse> GetCall<TResponse>(string urlSection, CancellationToken cancellationToken = default)
        {
            var endpointUrl = GetEndpointUrl(urlSection);

            var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl);

            return await _restClient.SendAsync<TResponse>(request, cancellationToken);
        }

        protected async Task<TResponse> PostCall<TPayload, TResponse>(string urlSection, TPayload payload = null, CancellationToken cancellationToken = default) where TPayload : class where TResponse : class
        {
            var endpointUrl = GetEndpointUrl(urlSection);

            var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl);

            if (payload is not null)
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            return await _restClient.SendAsync<TResponse>(request, cancellationToken);
        }

        protected async Task PostCall<TPayload>(string urlSection, TPayload payload = null, CancellationToken cancellationToken = default) where TPayload : class
        {
            var endpointUrl = GetEndpointUrl(urlSection);

            var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl);

            if (payload is not null)
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            await _restClient.SendVoidAsync(request, cancellationToken);
        }
    }
}
