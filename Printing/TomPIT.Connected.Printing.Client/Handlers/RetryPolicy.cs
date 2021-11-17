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
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            if (retryContext.ElapsedTime >= TimeSpan.FromHours(110))
            {
                return null;
            }

            if (retryContext.PreviousRetryCount >= 2880)
            {
                return null;
            }

            var nextDelay = (retryContext.PreviousRetryCount == 0)
                ? 0
                : 60;

            return TimeSpan.FromSeconds(nextDelay);
        }
    }
}
