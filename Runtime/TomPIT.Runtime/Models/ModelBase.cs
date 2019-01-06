using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.Runtime;
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

		public void Initialize(Controller controller)
		{
			Controller = controller;
			Request = Controller?.Request;

			var p = new ModelInitializeParams();

			OnInitializing(p);

			Initialize(controller?.Request, p.Endpoint, p.Authority, p.AuthorityId, p.ContextId);
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
