using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Models
{
	public class MailTemplateModel : ExecutionContext, IRuntimeModel
	{
		public MailTemplateModel(HttpRequest request, ActionContext context, JObject arguments)
		{
			ActionContext = context;
			Arguments = arguments;
		}

		public ActionContext ActionContext { get; }
		public JObject Arguments { get; }

		public void Initialize(IMicroService microService)
		{
			Initialize(null, microService);
		}
	}
}
