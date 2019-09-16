using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Models
{
	public class ModelBase : MiddlewareContext, IUIModel, IActionContextProvider
	{
		private IModelNavigation _navigation = null;

		public virtual IEnumerable<ValidationResult> Validate()
		{
			return null;
		}

		protected Controller Controller { get; private set; }

		public ActionContext ActionContext => Controller?.ControllerContext;

		protected void Initialize(Controller controller, IMicroService microService, bool initializing)
		{
			Controller = controller;
			MicroService = microService;

			var p = new ModelInitializeParams();

			if (initializing)
				OnInitializing(p);

			Initialize(string.IsNullOrWhiteSpace(p.Endpoint) ? Endpoint : p.Endpoint, p.MicroService == null ? MicroService : p.MicroService);
		}

		public void Initialize(Controller controller, IMicroService microService)
		{
			Initialize(controller, microService, true);
		}

		public void Databind()
		{
			OnDatabinding();
		}

		protected virtual void OnInitializing(ModelInitializeParams initializeParams)
		{
		}

		protected virtual void OnDatabinding()
		{

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

		protected HttpContext HttpContext => Controller?.HttpContext;
	}
}
