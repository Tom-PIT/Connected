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
		private IContextIdentity _identity = null;

		public RuntimeModel(RuntimeModel context)
		{
			ActionContext = context.ActionContext;

			Bind(context.Identity.AuthorityId, context.Identity.Authority, context.Identity.ContextId);
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

		public void Initialize(Controller controller)
		{
			Controller = controller;
			Initialize(null, null, null, null);

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

		public void Bind(string authorityId, string authority, string contextId)
		{
			_identity = this.CreateIdentity(authority, authorityId, contextId);
		}

		public override IContextIdentity Identity => _identity;

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
