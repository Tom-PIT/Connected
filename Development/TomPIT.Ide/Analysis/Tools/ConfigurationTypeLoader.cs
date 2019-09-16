using System;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Ide.Analysis.Diagnostics;
using TomPIT.Ide.Designers;

namespace TomPIT.Ide.Analysis.Tools
{
	internal class ConfigurationTypeLoader : IToolMiddleware
	{
		public string Name => "ConfigurationTypeLoader";

		public void Execute(ITenant tenant)
		{
			var microServices = tenant.GetService<IMicroServiceService>().Query();

			foreach (var microService in microServices)
			{
				var components = tenant.GetService<IComponentService>().QueryComponents(microService.Token);

				foreach (var component in components)
				{
					tenant.GetService<IDesignerService>().ClearErrors(component.Token, Guid.Empty, Development.ErrorCategory.Type);

					try
					{
						if (Type.GetType(component.Type, false) == null)
						{
							LogError(tenant, component, $"{SR.ErrCannotResolveComponentType} ({component.Type})", ErrorCodes.ComponentTypeResolve);
							continue;
						}
					}
					catch (Exception ex)
					{
						LogError(tenant, component, ex.Message, ErrorCodes.ComponentTypeResolve);
						continue;
					}

					try
					{
						var configuraton = tenant.GetService<IComponentService>().SelectConfiguration(component.Token);
					}
					catch (Exception ex)
					{
						LogError(tenant, component, ex.Message, ErrorCodes.ConfigurationLoad);
					}
				}
			}
		}

		private void LogError(ITenant tenant, IComponent component, string message, int code)
		{
			tenant.GetService<IDesignerService>().InsertErrors(component.Token, new System.Collections.Generic.List<Development.IDevelopmentError>
			{
				new DevelopmentError
				{
					Category = Development.ErrorCategory.Type,
					Code = code,
					Component = component.Token,
					Message = message,
					Severity = Development.DevelopmentSeverity.Error,
				}
			});
		}
	}
}