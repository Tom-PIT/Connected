﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TomPIT.Connectivity
{
    public interface IHttpConnection
    {
        T Post<T>(string url, HttpRequestArgs e = null);
        T Post<T>(string url, object content, HttpRequestArgs e = null);
        Task<T> PostAsync<T>(string url, object content, HttpRequestArgs e = null);
        T Post<T>(string url, HttpContent httpContent, HttpRequestArgs e = null);

        void Post(string url, HttpRequestArgs e = null);
        void Post(string url, object content, HttpRequestArgs e = null);
        void Post(string url, HttpContent httpContent, HttpRequestArgs e = null);

        T Get<T>(string url, HttpRequestArgs e = null);

        HttpClient GetClient(ICredentials credentials = null);

        HttpClient GetClient(Guid authenticationToken);

        HttpClient GetClient(string authenticationToken);

        HttpClient GetClient(string username, string password);

        string AuthenticationToken { get; }
    }

}
