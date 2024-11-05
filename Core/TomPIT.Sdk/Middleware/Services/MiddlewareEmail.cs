using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.ComponentModel;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Runtime;
using TomPIT.Serialization;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareEmail : MiddlewareObject, IMiddlewareEmail
	{

		public Guid Send(string from, string to, string subject, string body)
		{
			return Send(from, to, subject, body, null, 0);
		}

		public Guid Send(string from, string to, string subject, string body, Dictionary<string, object> headers, int attachmentCount)
		{
			return Context.Tenant.GetService<IMailService>().Enqueue(from, to, subject, body, headers, attachmentCount, MailFormat.Html, DateTime.UtcNow, DateTime.UtcNow.AddHours(24));
		}

		public string Create(string template, string user)
		{
			return Create(template, user, null);
		}

		public string Create(string template, string user, object arguments)
		{
			var descriptor = ComponentDescriptor.MailTemplate(Context, template);

			descriptor.Validate();

			var url = Context.Tenant.GetService<IRuntimeService>().Features.HasFlag(InstanceFeatures.Application)
				? Context.Services.Routing.RootUrl
				: Context.Tenant.GetService<IInstanceEndpointService>().Url(InstanceFeatures.Application, InstanceVerbs.Post);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException(SR.ErrNoAppServer);

			url = $"{url}/sys/mail-template/{descriptor.Component.Token}";

			var e = new JObject();

			if (!string.IsNullOrWhiteSpace(user))
				e.Add("user", user);

			if (arguments != null)
				e.Add("arguments", Serializer.Serialize(arguments));

			return Context.Tenant.Post<string>(url, e, new Connectivity.HttpRequestArgs
			{
				ReadRawResponse = true
			});
		}
	}
}
