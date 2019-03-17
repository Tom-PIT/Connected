using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Design.Services;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class StringTableStringProvider : CodeAnalysisProvider, ICodeAnalysisProvider
	{
		private Guid _microService = Guid.Empty;

		public StringTableStringProvider(IExecutionContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			if (!(e.Node.Parent is ArgumentListSyntax arg))
				return null;

			if (!(arg.Parent is InvocationExpressionSyntax invoke))
				return null;

			if (invoke.ArgumentList.Arguments.Count < 1)
				return null;

			var dsName = invoke.ArgumentList.Arguments[0].GetText().ToString().Trim().Trim('"');

			if (string.IsNullOrWhiteSpace(dsName))
				return null;

			var ds = context.Connection().GetService<IComponentService>().SelectComponent(context.MicroService.Token, "StringTable", dsName);

			if (ds == null)
				return null;

			if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(ds.Token) is IStringTable config))
				return null;

			var r = new List<ICodeAnalysisResult>();

			if (config.Strings.Count == 0)
			{
				r.Add(new NoSuggestionResult("string table contains not strings"));

				return r;
			}

			foreach (var i in config.Strings.OrderBy(f => f.Key))
				r.Add(new CodeAnalysisResult(i.Key, i.Key, StringUtils.EllipseString( i.DefaultValue, 64)));

			if (r.Count > 0)
				return r;

			return null;
		}
	}
}
