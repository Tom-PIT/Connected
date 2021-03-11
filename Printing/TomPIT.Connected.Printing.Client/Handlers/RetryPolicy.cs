/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using Microsoft.AspNetCore.SignalR.Client;
using System;

namespace TomPIT.Connected.Printing.Client.Handlers
{
    public class ConnectionRetryPolicy : IRetryPolicy
    {
        private readonly Random _randomGenerator = new Random();

        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            //if maximum retry time or count has been reached stop reconnecting
            if (retryContext.ElapsedTime >= TimeSpan.FromSeconds(Constants.MaxReconnectionTimeInSeconds))
            {
                return null;
            }

            if (retryContext.PreviousRetryCount >= Constants.MaxReconnectionRetries)
            {
                return null;
            }

            var nextDelay = _randomGenerator.Next(1, 10) * 5; //next delay is from 1 to 50 seconds
            return TimeSpan.FromSeconds(nextDelay);
        }
    }
}
