﻿using System;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.ComponentModel;
using TomPIT.Diagostics;
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

		public Guid Send(string from, string to, string subject, string body, JArray headers, int attachmentCount)
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

			var url = Context.Tenant.GetService<IRuntimeService>().Type == InstanceType.Application
				? Context.Services.Routing.RootUrl
				: Context.Tenant.GetService<IInstanceEndpointService>().Url(InstanceType.Application, InstanceVerbs.Post);

			if (string.IsNullOrWhiteSpace(url))
				throw new RuntimeException(SR.ErrNoAppServer).WithMetrics(Context);

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