using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using TomPIT.Storage;
using TomPIT.Sys.Exceptions;
using TomPIT.Sys.Net;
using TomPIT.Sys.Security;

namespace TomPIT.Sys
{
	public static class SysExtensions
	{
		public static AuthenticationBuilder AddTomPITAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<TomPITAuthenticationOptions> configureOptions)
		{
			return builder.AddScheme<TomPITAuthenticationOptions, TomPITAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
		}

		public static IApplicationBuilder UseTomPITExceptionMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<SysExceptionMiddleware>();
		}

		public static List<IClientQueueMessage> ToClientQueueMessage(this List<IQueueMessage> items, Guid resourceGroup)
		{
			var r = new List<IClientQueueMessage>();

			foreach (var i in items)
			{
				r.Add(new ClientQueueMessage
				{
					Created = i.Created,
					DequeueCount = i.DequeueCount,
					DequeueTimestamp = i.DequeueTimestamp,
					Expire = i.Expire,
					Id = i.Id,
					Message = i.Message,
					NextVisible = i.NextVisible,
					PopReceipt = i.PopReceipt,
					Queue = i.Queue,
					ResourceGroup = resourceGroup
				});
			}

			return r;
		}
	}
}
