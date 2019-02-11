using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Services;

namespace TomPIT.Models
{
	public class RuntimeModel : ExecutionContext, IRuntimeModel
	{
		private IModelNavigation _navigation = null;

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
	}
}
