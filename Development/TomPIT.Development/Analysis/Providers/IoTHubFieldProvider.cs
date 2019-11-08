using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.Ide.Analysis;
using TomPIT.Ide.Analysis.Suggestions;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class IoTHubFieldProvider : CodeAnalysisProvider
	{
		public IoTHubFieldProvider(IMiddlewareContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			if (!(e.Node.Parent is ArgumentListSyntax arg))
				return null;

			if (!(arg.Parent is InvocationExpressionSyntax invoke))
				return null;

			if (invoke.ArgumentList.Arguments.Count < 1)
				return null;

			var hub = invoke.ArgumentList.Arguments[0].GetText().ToString().Trim().Trim('"');

			if (string.IsNullOrWhiteSpace(hub))
				return null;

			var tokens = hub.Split('/');
			var ms = e.Component.MicroService;

			if (tokens.Length > 1)
			{
				var microService = context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

				if (microService == null)
					return null;

				ms = microService.Token;
			}

			var hubComponent = context.Tenant.GetService<IComponentService>().SelectComponent(ms, ComponentCategories.IoTHub, tokens[1]);

			if (hubComponent == null)
				return null;

			if (!(context.Tenant.GetService<IComponentService>().SelectConfiguration(hubComponent.Token) is IIoTHubConfiguration config))
				return null;

			if (string.IsNullOrWhiteSpace(config.Schema))
				return null;

			if (!(context.Tenant.GetService<IComponentService>().SelectConfiguration(ms, ComponentCategories.IoTSchema, config.Schema) is IIoTSchemaConfiguration iotSchema))
				return null;

			var r = new List<ICodeAnalysisResult>();

			foreach (var i in iotSchema.Fields)
			{
				if (string.IsNullOrWhiteSpace(i.Name))
					continue;

				r.Add(new CodeAnalysisResult($"{i.Name} ({i.DataType})", i.Name, null));
			}

			if (r.Count > 0)
				return r;

			if (iotSchema.Fields.Count == 0)
			{
				r.Add(new NoSuggestionResult("no schema fields"));

				return r;
			}

			return null;
		}
	}
}
