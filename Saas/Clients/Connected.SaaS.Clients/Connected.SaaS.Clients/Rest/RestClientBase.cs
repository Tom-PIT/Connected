using Connected.SaaS.Clients.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace Connected.SaaS.Clients.Rest
{
    public abstract class RestClientBase : HttpClient, IRestClient, IAsyncDisposable
    {
        private readonly IRestAuthenticationProvider _authenticationProvider;

        public IRestAuthenticationProvider AuthenticationProvider => _authenticationProvider;

        public RestClientBase(string endpointUrl, IRestAuthenticationProvider restAuthenticationProvider)
        {
            if (string.IsNullOrWhiteSpace(endpointUrl))
                throw new ArgumentNullException(nameof(endpointUrl));

            if (restAuthenticationProvider is null)
                throw new ArgumentNullException(nameof(restAuthenticationProvider));

            this.EndpointUrl = endpointUrl;
            this._authenticationProvider = restAuthenticationProvider;
        }

        public string EndpointUrl { get; }

        public override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _authenticationProvider.Apply(request);
            return base.Send(request, cancellationToken);
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _authenticationProvider.Apply(request);
            return await base.SendAsync(request, cancellationToken);
        }

        public string GetEndpointUrl(string endpoint)
        {
            return $"{this.EndpointUrl}/{endpoint}";
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage message, CancellationToken cancellationToken = default)
        {
            return await HandleResponseAsync<T>(await SendAsync(message, cancellationToken));
        }

        public async Task SendVoidAsync(HttpRequestMessage message, CancellationToken cancellationToken = default)
        {
            await HandleResponseAsync(await SendAsync(message, cancellationToken));
        }

        private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                await HandleResponseExceptionAsync(response);

            var content = await response.Content.ReadAsStringAsync();

            if (IsNull(content))
                return default;

            return JsonSerializer.Deserialize<T>(content);
        }

        private async Task HandleResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                await HandleResponseExceptionAsync(response);            
        }

        private async Task HandleResponseExceptionAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                string error;
                try
                {
                    error = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    error = "Unknown error occurred";
                }

                throw new Exception($"Server-side problem occured: ({response.ReasonPhrase} {error})");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception(response.ReasonPhrase);

            var rt = string.Empty;

            if (response.Content != null)
                rt = await response.Content.ReadAsStringAsync();

            throw new Exception(rt);
        }

        private static bool IsNull(string content)
        {
            return string.Compare(content, "null", true) == 0
                || string.IsNullOrWhiteSpace(content);
        }

        public async ValueTask DisposeAsync()
        {
            this.TryDispose();
            await Task.CompletedTask;
        }
    }
}
