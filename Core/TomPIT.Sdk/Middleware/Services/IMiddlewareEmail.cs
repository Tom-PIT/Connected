using System;
using System.Collections.Generic;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareEmail
	{
		Guid Send(string from, string to, string subject, string body);
		Guid Send(string from, string to, string subject, string body, Dictionary<string, object> headers, int attachmentCount);

		string Create([CIP(CIP.MailTemplateProvider)]string template, string user);
		string Create([CIP(CIP.MailTemplateProvider)]string template, string user, object arguments);
	}
}
