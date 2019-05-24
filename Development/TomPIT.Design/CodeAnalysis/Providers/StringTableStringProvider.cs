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

			var stringTable = invoke.ArgumentList.Arguments[0].GetText().ToString().Trim().Trim('"');

			if (string.IsNullOrWhiteSpace(stringTable))
				return null;

			var microService = context.MicroService;

			if(stringTable.Contains('/'))
			{
				var tokens = stringTable.Split('/');

				context.MicroService.ValidateMicroServiceReference(context.Connection(), tokens[0]);

				microService = context.Connection().GetService<IMicroServiceService>().Select(tokens[0]);

				if (microService == null)
					throw new Exception($"{SR.ErrMicroServiceNotFound} ({microService})");

				stringTable = tokens[1];
			}

			var ds = context.Connection().GetService<IComponentService>().SelectComponent(microService.Token, "StringTable", stringTable);

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

		public override List<ICodeAnalysisResult> ProvideHover(IExecutionContext context, CodeAnalysisArgs e)
		{
			if (!(e.Node is ArgumentSyntax arg))
				return null;

			var list = e.Node.Parent as ArgumentListSyntax;

			var table = list.Arguments[0].GetText().ToString();
			var str = e.Node.GetText().ToString();

			if (string.IsNullOrWhiteSpace(table) || string.IsNullOrWhiteSpace(str))
				return null;

			var value = context.Services.Localization.GetString(table.Substring(1, table.Length-2), str.Substring(1, str.Length-2));

			if (string.IsNullOrWhiteSpace(value))
				return null;

			var r = new List<ICodeAnalysisResult>
			{
				new CodeAnalysisResult( value)
			};

			return r;
		}
	}
}
