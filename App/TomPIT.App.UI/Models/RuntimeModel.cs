using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Services;

namespace TomPIT.Models
{
	public class RuntimeModel : ExecutionContext, IViewModel, IComponentModel
	{
		private IModelNavigation _navigation = null;
		private JObject _arguments = null;

		public RuntimeModel(RuntimeModel context) : base(context)
		{
			ActionContext = context.ActionContext;
		}

		public RuntimeModel(HttpRequest request, ActionContext context)
		{
			ActionContext = context;
		}

		public IComponent Component { get; set; }

		public IView ViewConfiguration { get; set; }

		public virtual IEnumerable<ValidationResult> Validate()
		{
			return null;
		}

		protected Controller Controller { get; private set; }
		public ActionContext ActionContext { get; }

		public void Initialize(Controller controller, IMicroService microService)
		{
			Controller = controller;

			base.Initialize(null, microService);

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
	}
}
