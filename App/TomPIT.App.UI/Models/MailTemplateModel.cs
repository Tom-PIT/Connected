using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Services;

namespace TomPIT.Models
{
	public class MailTemplateModel : ExecutionContext, IViewModel
	{
		public MailTemplateModel(HttpRequest request, ActionContext context, JObject arguments)
		{
			ActionContext = context;
			Arguments = arguments;
		}

		public ActionContext ActionContext { get; }
		public JObject Arguments { get; }

		public IView ViewConfiguration => null;

		public IModelNavigation Navigation => null;

		public string Title => null;

		public IComponent Component { get; set; }

		public void Initialize(IMicroService microService)
		{
			Initialize(null, microService);
		}

		public void Initialize(Controller controller, IMicroService microService)
		{
			Initialize(Instance.Connection.Url, microService);
		}

		public void MergeArguments(JObject arguments)
		{
			if (arguments != null)
				Arguments.Merge(arguments);
		}

		public IEnumerable<ValidationResult> Validate()
		{
			return null;
		}
	}
}
