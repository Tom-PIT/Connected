using System;
using Newtonsoft.Json.Linq;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareEmail
	{
		Guid Send(string from, string to, string subject, string body);
		Guid Send(string from, string to, string subject, string body, JArray headers, int attachmentCount);

		string Create([CAP(CAP.MailTemplateProvider)]string template, string user);
		string Create([CAP(CAP.MailTemplateProvider)]string template, string user, object arguments);
	}
}
