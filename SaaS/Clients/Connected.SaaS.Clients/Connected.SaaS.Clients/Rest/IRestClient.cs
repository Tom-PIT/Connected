using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connected.SaaS.Clients.Rest
{
    public interface IRestClient
    {
        string EndpointUrl { get; }

        Task<T> SendAsync<T>(HttpRequestMessage message, CancellationToken cancellationToken = default);

        Task SendVoidAsync(HttpRequestMessage message, CancellationToken cancellationToken = default);
    }
}
