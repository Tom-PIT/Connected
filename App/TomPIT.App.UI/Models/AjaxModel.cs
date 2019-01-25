using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Services;

namespace TomPIT.Models
{
	public class AjaxModel : ExecutionContext, IModel, IRequestContextProvider
	{
		private IContextIdentity _identity = null;
		private IComponent _component = null;

		public string QualifierName { get; protected set; }
		internal JObject Body { get; set; }

		public JObject Arguments { get { return Body; } }

		public override IContextIdentity Identity
		{
			get
			{
				if (_identity == null)
				{

					var microService = Connection.GetService<IMicroServiceService>().Select(Component.MicroService);

					if (microService == null)
						throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, Component.MicroService.ToString()));

					_identity = this.CreateIdentity(null, null, microService.Token.ToString());
				}

				return _identity;
			}
		}

		protected void SetIdentity(IContextIdentity identity)
		{
			_identity = identity;
		}

		private IComponent Component
		{
			get
			{
				if (_component == null)
				{
					var s = Body.Optional("__component", string.Empty);

					if (s == string.Empty)
						throw ExecutionException.ParameterExpected(this, new ExecutionContextState(), "__component");

					var tokens = s.Split('.');

					_component = Connection.GetService<IComponentService>().SelectComponent(tokens[1].AsGuid());
				}

				return _component;
			}
		}

		public virtual IEnumerable<ValidationResult> Validate()
		{
			return null;
		}

		protected Controller Controller { get; private set; }

		public ActionContext ActionContext => Controller?.ControllerContext;

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
	}
}
