using Connected.SaaS.Clients.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Connected.SaaS.Clients.Authentication
{
    public interface IRestAuthenticationProvider
    {
        void Apply(HttpRequestMessage message);
    }
}
