using System;
using System.Net;
using Microsoft.AspNetCore.Mvc.Razor;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Serialization;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Runtime.UI
{
	public abstract class ViewBase<T> : RazorPage<T>
	{
		private IConfiguration _configuration = null;

		protected Guid ComponentId { get; set; }
		protected string ViewType { get; set; }

		private IViewModel ViewModel => Model as IViewModel;
		private IConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(ComponentId);

				return _configuration;
			}
		}

		protected string GetString([CIP(CIP.StringTableProvider)] string stringTable, [CIP(CIP.StringTableStringProvider)] string key)
		{
			return ViewModel.Services.Globalization.GetString(stringTable, key);
		}

		protected string ToJsonString(object content)
		{
			if (content == null)
				return "null";

			return Serializer.Serialize(content);
		}

		protected void NotFound()
		{
			Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
		}

		protected void BadRequest()
		{
			Context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
		}
		protected void Deny()
		{
			if (ViewModel.Services.Identity.User == null)
				Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			else
				Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
		}

	}
}