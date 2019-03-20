using System.Collections.Generic;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.Design.Services;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class IoTHubProvider : CodeAnalysisProvider
	{
		public IoTHubProvider(IExecutionContext context) : base(context)
		{

		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var ds = context.Connection().GetService<IComponentService>().QueryComponents(e.Component.MicroService, "IoTHub");
			var r = new List<ICodeAnalysisResult>();

			foreach (var i in ds)
				r.Add(new CodeAnalysisResult(i.Name, i.Name, string.Empty));

			var msApis = new List<ICodeAnalysisResult>();

			var refs = context.Connection().GetService<IDiscoveryService>().References(e.Component.MicroService);

			foreach (var i in refs.MicroServices)
			{
				var ms = context.Connection().GetService<IMicroServiceService>().Select(i.MicroService);

				if (ms == null)
					continue;

				ds = context.Connection().GetService<IComponentService>().QueryComponents(ms.Token, "IoTHub");

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
