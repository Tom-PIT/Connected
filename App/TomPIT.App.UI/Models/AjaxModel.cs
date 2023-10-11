using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Models;

namespace TomPIT.App.Models
{
	public class AjaxModel : MicroServiceContext, IModel, IActionContextProvider, IRuntimeModel
	{
		private IComponent _component = null;

		public string QualifierName { get; protected set; }
		internal JObject Body { get; set; }

		public JObject Arguments { get { return Body; } }

		private IComponent Component
		{
			get
			{
				if (_component == null)
				{
					var s = Body.Optional("__component", string.Empty);

					if (s == string.Empty)
						throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataParameterExpected, "__component"));

					var tokens = s.Split('.');

					_component = Tenant.GetService<IComponentService>().SelectComponent(new Guid(tokens[1]));
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

		public void Initialize(Controller controller, IMicroService microService)
		{
			Controller = controller;
			MicroService = microService;

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

		public void ReplaceArguments(JObject arguments)
		{
			if (arguments is not null)
				Body = arguments;
			else
				Body = new();
		}

		public virtual IRuntimeModel Clone()
		{
			var ajaxModel = new AjaxModel
			{
				Body = (JObject)Body.DeepClone()
			};

			ajaxModel.Initialize(Controller, MicroService);

			return ajaxModel;
		}
	}
}
