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
	public class RuntimeModel : MicroServiceContext, IViewModel, IComponentModel
	{
		private IModelNavigation _navigation = null;
		private JObject _arguments = null;

		public RuntimeModel(RuntimeModel context) : base(context)
		{
			ActionContext = context.ActionContext;
			TempData = context.TempData;
			MicroService = context.MicroService;
		}

		public RuntimeModel(HttpRequest request, ActionContext context, ITempDataProvider tempData, IMicroService microService)
		{
			ActionContext = context;
			TempData = tempData;
			MicroService = microService;
		}

		public IComponent Component { get; set; }

		public IViewConfiguration ViewConfiguration { get; set; }

		public virtual IEnumerable<ValidationResult> Validate()
		{
			return null;
		}

		protected Controller Controller { get; private set; }
		public ActionContext ActionContext { get; }

		public void Initialize(Controller controller, IMicroService microService)
		{
			Controller = controller;
			MicroService = microService;

			Initialize(null);

			OnInitializing();
		}

		public void Databind()
		{
			OnDatabinding();
		}

		protected virtual void OnInitializing()
		{

		}

		protected virtual void OnDatabinding()
		{

		}

		public void MergeArguments(JObject arguments)
		{
			if (arguments != null)
				Arguments.Merge(arguments);
		}

		public string Title { get; protected set; }

		public IModelNavigation Navigation
		{
			get
			{
				if (_navigation == null)
					_navigation = new ModelNavigation();

				return _navigation;
			}
		}

		public JObject Arguments
		{
			get
			{
				if (_arguments == null)
				{
					_arguments = new JObject();

					foreach (var i in ActionContext.RouteData.Values)
						_arguments.Add(i.Key, Types.Convert<string>(i.Value));

					foreach (var i in ActionContext.HttpContext.Request.Query)
					{
						if (_arguments.ContainsKey(i.Key))
							continue;

						_arguments.Add(i.Key, i.Value.ToString());
					}
				}

				return _arguments;
			}
		}

		public ITempDataProvider TempData { get; }
	}
}
