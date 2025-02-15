﻿using System;
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
		public JObject Arguments { get; internal set; }

		public IViewConfiguration ViewConfiguration => null;

		public IModelNavigation Navigation => null;

		public string Title => null;

		public IComponent Component { get; set; }

		public ITempDataProvider TempData { get; }

        public IRuntimeModel Clone()
        {
			var mailTemplateModel = new MailTemplateModel(null, ActionContext, TempData, (JObject)Arguments?.DeepClone());
			return mailTemplateModel;
        }

        public void Initialize(IMicroService microService)
		{
			Initialize(null, microService);
		}

		public void Initialize(Controller controller, IMicroService microService)
		{
			MicroService = microService;
		}

		public void MergeArguments(JObject arguments)
		{
			if (arguments != null)
				Arguments.Merge(arguments);
		}

        public void ReplaceArguments(JObject arguments)
        {
			throw new NotImplementedException();
        }

        public IEnumerable<ValidationResult> Validate()
		{
			return null;
		}
	}
}
