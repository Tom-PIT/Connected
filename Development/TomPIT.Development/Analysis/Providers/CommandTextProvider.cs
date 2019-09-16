using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel.Data;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class CommandTextProvider : CodeAnalysisProvider
	{
		public CommandTextProvider(IMiddlewareContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			var parent = e.Node.Parent as ArgumentListSyntax;
			var argument = parent.Arguments[0];
			IConnectionConfiguration connection = null;

			if (argument.Expression is LiteralExpressionSyntax le)
				connection = le.ResolveConnection(Context);
			else if (argument.Expression is IdentifierNameSyntax ins)
				connection = ins.ResolveConnection(Context);

			if (connection == null)
				connection = context.DefaultConnection();

			if (connection == null)
				return null;

			var browser = connection.ResolveSchemaBrowser(Context);

			if (browser == null)
				return null;

			var r = new List<ICodeAnalysisResult>();
			var objects = browser.QueryGroupObjects(connection);

			if (objects == null)
				return r;

			foreach (var o in objects)
				r.Add(new CodeAnalysisResult(o.Text, o.Value, o.Description));

			return r;
		}
	}
}
