using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Models
{
	public class ModelBase : ExecutionContext, IUIModel, IRequestContextProvider
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

			var p = new ModelInitializeParams();

			if (initializing)
				OnInitializing(p);

			Initialize(p.Endpoint, p.MicroService);
		}

		public void Initialize(Controller controller, IMicroService microService)
		{
			Initialize(controller, MicroService, true);
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
