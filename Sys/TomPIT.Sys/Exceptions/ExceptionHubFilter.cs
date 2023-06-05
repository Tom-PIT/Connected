using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TomPIT.Diagnostics;

namespace TomPIT.Sys.Exceptions
{
    public class ExceptionHubFilter : IHubFilter
    {
        public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            try
            {
                return await next(invocationContext);
            }
            catch (Exception ex)
            {
                //TODO log
                EventLog.WriteWarning(ex.ToString());
                return default;
            }
        }
    }
}
