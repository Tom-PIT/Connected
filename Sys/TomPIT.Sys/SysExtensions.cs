using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using TomPIT.ComponentModel;
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

				return new Guid(instance.ToString());
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

		public static void DemandDevelopmentStage(this IMicroService microService)
		{
			if (microService.Status == MicroServiceStatus.Production)
				throw new SysException(SR.ErrProductionStage);
			else if (microService.Status == MicroServiceStatus.Staging)
				throw new SysException(SR.ErrStagingStage);
		}

		public static void DemandDevelopmentStage(this IComponent component)
		{
			DataModel.MicroServices.Select(component.MicroService).DemandDevelopmentStage();
		}
	}
}
