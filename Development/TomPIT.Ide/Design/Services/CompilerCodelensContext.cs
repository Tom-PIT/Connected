using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Design.Services
{
	internal class CompilerCodeLensContext : Analyzer<CodeLensArgs>
	{
		private ICodeLens _codelens = null;

		public CompilerCodeLensContext(IExecutionContext context, CodeLensArgs e) : base(context, e)
		{
		}

		public ICodeLens CodeLens
		{
			get
			{
				if (_codelens == null)
					_codelens = CreateCodeLens();

				return _codelens;
			}
		}

		private ICodeLens CreateCodeLens()
		{
			var r = new CodeLens();

			var sm = Task.Run(async () => { return await Document.GetSemanticModelAsync(); }).Result;
			var nodes = sm.SyntaxTree.GetRoot().DescendantNodes();

			foreach (var i in nodes)
			{
				if (!(i is ArgumentSyntax args))
					continue;

				var list = GetArgumentList(args);

				if (list == null)
					continue;

				var mi = GetMethodInfo(sm, args);

				if (mi == null)
					continue;

				var parameter = GetParameter(mi, list, args);

				if (parameter == null)
					continue;

				var provider = GetProvider(parameter);

				if (provider != null)
				{
					var expr = GetArgumentExpression(args);

					if (expr == null)
						continue;

					var text = ParseExpressionText(expr);

					if (string.IsNullOrWhiteSpace(text))
						continue;

					var cl = provider.CodeLens(Context, new CodeAnalysisArgs(Args.Component, sm, i, GetInvocationSymbolInfo(sm, args), text));

					if (cl != null)

						r.Items.Add(new CodeLensDescriptor
						{
							Id = cl.Text,
							Command = cl.Command,
							Range = (Range)args
						});
				}
			}

			return r;
		}
	}
}
