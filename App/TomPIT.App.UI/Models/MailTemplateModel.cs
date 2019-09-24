using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Middleware;
using TomPIT.Models;

namespace TomPIT.App.Models
{
	public class MailTemplateModel : MicroServiceContext, IViewModel
	{
		public MailTemplateModel(HttpRequest request, ActionContext context, ITempDataProvider tempData, JObject arguments)
		{
			ActionContext = context;
			Arguments = arguments;
			TempData = tempData;
		}

		public ActionContext ActionContext { get; }
		public JObject Arguments { get; }

		public IViewConfiguration ViewConfiguration => null;

		public IModelNavigation Navigation => null;

		public string Title => null;

		public IComponent Component { get; set; }

		public ITempDataProvider TempData { get; }

		public void Initialize(IMicroService microService)
		{
			Initialize(null, microService);
		}

		public void Initialize(Controller controller, IMicroService microService)
		{
			MicroService = microService;

			Initialize(null);
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
