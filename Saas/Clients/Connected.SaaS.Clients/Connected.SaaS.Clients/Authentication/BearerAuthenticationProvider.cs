using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Connected.SaaS.Clients.Authentication
{
    public class BearerAuthenticationProvider : IRestAuthenticationProvider
    {
        private readonly string _token;
        public BearerAuthenticationProvider(string token)
        {
            _token = token;
        }

        public void Apply(HttpRequestMessage message)
        {
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Convert.ToBase64String(Encoding.UTF8.GetBytes(_token)));
        }

        public override int GetHashCode()
        {
            return (_token?.GetHashCode() ?? 0) + GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is not BearerAuthenticationProvider second)
                return false;

            return second._token == _token;
        }
    }
}
