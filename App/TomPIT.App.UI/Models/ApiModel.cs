using Newtonsoft.Json.Linq;
using System;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Models;

namespace TomPIT.App.Models
{
	public class ApiModel : AjaxModel
	{
		public ApiModel()
		{
			Shell.HttpContext.Items["RootModel"] = this;
		}

		public ApiModel(string api, string component) : this()
		{
			if (string.IsNullOrWhiteSpace(api))
				throw new RuntimeException($"{SR.ErrHttpHeaderExpected} (X-TP-API)");

			if (string.IsNullOrWhiteSpace(component))
				throw new RuntimeException($"{SR.ErrHttpHeaderExpected} (X-TP-COMPONENT)");

			QualifierName = api;

			var tokens = component.Split('.');

			if (tokens.Length != 2)
				throw new RuntimeException($"{SR.ErrInvalidQualifier} ({component}). {SR.ErrInvalidQualifierExpected}: 'microService.component'.");

			var s = Tenant.GetService<IMicroServiceService>().Select(new Guid(tokens[0]));
			var c = Tenant.GetService<IComponentService>().SelectComponent(new Guid(tokens[1]));

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			MicroService = s;
		}

		protected override void OnDatabinding()
		{
			if (Body == null && MicroService != null)
				return;

			if (string.IsNullOrWhiteSpace(Body.Optional("__api", string.Empty)))
				throw new RuntimeException($"{SR.ErrDataParameterExpected} (__api)");

			QualifierName = Body.Optional("__api", string.Empty);

			var component = Body.Optional("__component", string.Empty);
			var tokens = component.Split('.');

			if (tokens.Length != 2)
				throw new RuntimeException($"{SR.ErrInvalidQualifier} ({component}). {SR.ErrInvalidQualifierExpected}: 'microService.component'.");

			var s = Tenant.GetService<IMicroServiceService>().Select(new Guid(tokens[0]));
			var c = Tenant.GetService<IComponentService>().SelectComponent(new Guid(tokens[1]));

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			Body.Remove("__api");
			Body.Remove("__component");

			MicroService = s;
		}

		protected override void OnInitializing()
		{
			base.OnInitializing();

			if (Body == null)
			{
				Body = new JObject();

				if (Controller == null)
					return;

				foreach (var header in Controller.Request.Headers)
				{
					if (!header.Key.StartsWith(HttpExtensions.HeaderParamPrefix, StringComparison.OrdinalIgnoreCase))
						continue;

					var key = header.Key.Substring(HttpExtensions.HeaderParamPrefix.Length);

					Body.Add(new JProperty(key, header.Value.ToString()));
				}
			}
		}

		public override IRuntimeModel Clone()
		{
			return this;
		}
	}
}
