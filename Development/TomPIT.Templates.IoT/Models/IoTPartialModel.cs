using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Models;
using TomPIT.Services;

namespace TomPIT.IoT.Models
{
	internal class IoTPartialModel : ExecutionContext, IRuntimeModel
	{
		private IModelNavigation _navigation = null;
		private IContextIdentity _identity = null;

		public IoTPartialModel(ActionContext context)
		{
			//ActionContext = context.ActionContext;

			//Bind(context.Identity.AuthorityId, context.Identity.Authority, context.Identity.ContextId);
		}

		public IComponent Component { get; set; }
		public IView ViewConfiguration { get; set; }

		protected Controller Controller { get; private set; }
		public ActionContext ActionContext { get; }

		public void Initialize(Controller controller)
		{
			Controller = controller;
			Initialize(null, null, null, null);
		}

		public IEnumerable<ValidationResult> Validate()
		{
			return null;
		}

		public void Bind(string authorityId, string authority, string contextId)
		{

		}

		public override IContextIdentity Identity => _identity;
		public string Title { get; protected set; }
		public IModelNavigation Navigation => null;
	}
}
