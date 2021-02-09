/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TomPIT.Sdk.HealthMonitoring;

namespace TomPIT.Sdk.Base
{
    public abstract partial class BaseConnection : IEndPointHealthMonitoring
    {
        private string _serverUrl = string.Empty;
        private string _authToken = string.Empty;
        private string _name = string.Empty;

        private IRetryPolicy _connectionRetryPolicy = null;

        protected HubConnection Connection;

        public BaseConnection SetUrl(string url)
        {
            _serverUrl = url;

            return this;
        }

        public BaseConnection SetToken(string token)
        {
            _authToken = token;

            return this;
        }

        public BaseConnection SetName(string name)
        {
            _name = name;

            return this;
        }

        public BaseConnection SetRetryPolicy(IRetryPolicy retryPolicy)
        {
            _connectionRetryPolicy = retryPolicy;

            return this;
        }

        public void Create()
        {
            Connection = new HubConnectionBuilder().WithUrl(_serverUrl, o =>
            {
                o.AccessTokenProvider = async () =>
                {
                    return await Task.FromResult(_authToken);
                };
            }).WithAutomaticReconnect(_connectionRetryPolicy ?? new DefaultConnectionRetryPolicy())
                .AddNewtonsoftJsonProtocol()
                .Build();

            Connection.Reconnected += async (arg) =>
            {
                await OnReconnected(arg);
            };
        }

        async protected virtual Task OnReconnected(string connectionId)
        {
            await Task.CompletedTask;
        }
    }
}
