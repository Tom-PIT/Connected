using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using TomPIT.Sys.Data;
using TomPIT.Sys.Exceptions;
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

		public static Guid RequestInstanceId
		{
			get
			{
				if (Shell.HttpContext == null)
					return Guid.Empty;

				var instance = Shell.HttpContext.Request.Headers["TomPITInstanceId"];

				if (string.IsNullOrWhiteSpace(instance))
					return Guid.Empty;

				return instance.ToString().AsGuid();
			}
		}

		public static string RequestConnectionId(string topic)
		{
			if (RequestInstanceId == Guid.Empty)
				return null;

			var subscriber = DataModel.MessageSubscribers.Select(topic, RequestInstanceId);

			return subscriber?.Connection;
		}

		public static List<Guid> ToResourceGroupList(this List<string> items)
		{
			var r = new List<Guid>();

			foreach (var i in items)
			{
				var rg = DataModel.ResourceGroups.Select(i);

				if (rg == null)
					throw new SysException(string.Format("{0} ({1})", SR.ErrResourceGroupNotFound, i));

				r.Add(rg.Token);
			}

			return r;
		}
	}
}
