using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Diagostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.UI;

namespace TomPIT.App.Models
{
	public class PartialModel : AjaxModel, IViewModel
	{
		public IViewConfiguration ViewConfiguration => null;

		public IModelNavigation Navigation => null;
		public string Title => null;
		public IComponent Component { get; set; }

		public ITempDataProvider TempData { get; }

		protected override void OnDatabinding()
		{
			if (string.IsNullOrWhiteSpace(Body.Optional("__name", string.Empty)))
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataParameterExpected, "__name"))
				{
					Event = MiddlewareEvents.DataRead,
				}.WithMetrics(this);
			}

			QualifierName = Body.Optional("__name", string.Empty);

			if (string.IsNullOrWhiteSpace(Body.Optional("__component", string.Empty)))
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataParameterExpected, "__component"))
				{
					Event = MiddlewareEvents.DataRead,
				}.WithMetrics(this);
			}

			var component = Body.Optional("__component", string.Empty);
			var tokens = component.Split('.');

			if (tokens.Length != 2)
			{
				throw new RuntimeException(string.Format("{0} ({1}). {2}: {3}.", SR.ErrInvalidQualifier, component, SR.ErrInvalidQualifierExpected, "microService.component"))
				{
					Event = MiddlewareEvents.DataRead
				}.WithMetrics(this);
			}

			var s = Tenant.GetService<IMicroServiceService>().Select(new Guid(tokens[0]));

			if (s == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var c = Tenant.GetService<IComponentService>().SelectComponent(new Guid(tokens[1]));

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			MicroService = s;

			IPartialViewConfiguration partial = null;

			if (QualifierName.Contains('/'))
			{
				var partialTokens = QualifierName.Split('/');

				MicroService.ValidateMicroServiceReference(partialTokens[0]);
				var partialMs = Tenant.GetService<IMicroServiceService>().Select(partialTokens[0]);

				partial = Tenant.GetService<IComponentService>().SelectConfiguration(partialMs.Token, "Partial", partialTokens[1]) as IPartialViewConfiguration;
			}
			else
				partial = Tenant.GetService<IComponentService>().SelectConfiguration(MicroService.Token, "Partial", QualifierName) as IPartialViewConfiguration;

			var args = new ViewInvokeArguments(this);

			Body.Remove("__name");
			Body.Remove("__component");

			Tenant.GetService<ICompilerService>().Execute(((IConfiguration)partial).MicroService(), partial.Invoke, this, args);
		}
	}
}
