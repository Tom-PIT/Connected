using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Design.Services;
using TomPIT.Ide.CodeAnalysis;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class CommandTextProvider : CodeAnalysisProvider
	{
		public CommandTextProvider(IExecutionContext context) : base(context)
		{
		}

		public override List<ICodeAnalysisResult> ProvideLiterals(IExecutionContext context, CodeAnalysisArgs e)
		{
			var parent = e.Node.Parent as ArgumentListSyntax;
			var argument = parent.Arguments[0];
			IConnection connection = null;

			if (argument.Expression is LiteralExpressionSyntax le)
				connection = le.ResolveConnection(Context);
			else if (argument.Expression is IdentifierNameSyntax ins)
				connection = ins.ResolveConnection(Context);

			if (connection == null)
				connection = DefaultConnection();

			var browser = connection.ResolveSchemaBrowser(Context);

			if (browser == null)
				return null;

			var r = new List<ICodeAnalysisResult>();
			var objects = browser.QueryGroupObjects(connection);

			if (objects == null)
				return r;

			foreach (var o in objects)
				r.Add(new CodeAnalysisResult(o, o, null));

			return r;
		}

		private IConnection DefaultConnection()
		{
			var connections = Context.Connection().GetService<IComponentService>().QueryComponents(Context.MicroService.Token);

			if (connections != null && connections.Count == 0)
				return Context.Connection().GetService<IComponentService>().SelectConfiguration(connections[0].Token) as IConnection;

			return null;
		}
	}
}
