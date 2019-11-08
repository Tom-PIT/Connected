using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Development.Analysis.Providers
{
	internal class IoTHubProvider : CodeAnalysisProvider
	{
		public IoTHubProvider(IMiddlewareContext context) : base(context)
		{

		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			var ds = context.Tenant.GetService<IComponentService>().QueryComponents(e.Component.MicroService, ComponentCategories.IoTHub);
			var r = new List<ICodeAnalysisResult>();

			foreach (var i in ds)
				r.Add(new CodeAnalysisResult(i.Name, i.Name, string.Empty));

			var msApis = new List<ICodeAnalysisResult>();

			var refs = context.Tenant.GetService<IDiscoveryService>().References(e.Component.MicroService);

			foreach (var i in refs.MicroServices)
			{
				var ms = context.Tenant.GetService<IMicroServiceService>().Select(i.MicroService);

				if (ms == null)
					continue;

				ds = context.Tenant.GetService<IComponentService>().QueryComponents(ms.Token, ComponentCategories.IoTHub);

				foreach (var j in ds)
				{
					var key = $"{i.MicroService}/{j.Name}";

					r.Add(new CodeAnalysisResult(key, key, string.Empty));
				}
			}

			return r;
		}
	}
}
