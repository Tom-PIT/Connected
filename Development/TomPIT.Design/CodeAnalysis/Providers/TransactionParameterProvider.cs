using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Design.Services;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class TransactionParameterProvider : CodeAnalysisProvider
	{
		private SourceText _sourceCode = null;
		private Guid _microService = Guid.Empty;

		public TransactionParameterProvider(IExecutionContext context) : base(context)
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

			var method = GetMethodInfo(e.Model, e.Node as ArgumentSyntax);

			if (method == null)
				return null;

			var parameters = method.GetParameters();
			var parameterIndex = -1;

			for (var i = 0; i < parameters.Length; i++)
			{
				if (string.Compare(parameters[i].Name, "transaction", true) == 0)
				{
					parameterIndex = i;
					break;
				}
			}

			if (parameterIndex == -1)
				return null;

			var tName = invoke.ArgumentList.Arguments[parameterIndex].GetText().ToString().Trim().Trim('"');

			if (string.IsNullOrWhiteSpace(tName))
				return null;

			var tr = context.Connection().GetService<IComponentService>().SelectComponent(context.MicroService(), "Transaction", tName);

			if (tr == null)
				return null;

			var config = context.Connection().GetService<IComponentService>().SelectConfiguration(tr.Token) as ITransaction;

			if (config == null)
				return null;

			var existing = Existing(e.Node as ArgumentSyntax);
			var r = new List<ICodeAnalysisResult>();

			foreach (var i in config.Parameters.OrderBy(f => f.Name))
			{
				if (i is IReturnValueParameter)
					continue;

				if (existing != null && existing.Contains(i.Name.ToLowerInvariant()))
					continue;

				r.Add(new CodeAnalysisResult(string.Format("{0} ({1})", i.Name, i.DataType), i.Name, i.IsNullable ? "optional" : "required"));
			}

			if (r.Count > 0)
				return r;

			return null;
		}

		private List<string> Existing(ArgumentSyntax node)
		{
			var create = node.ChildNodes().FirstOrDefault(f => f is ObjectCreationExpressionSyntax);

			if (create == null)
				return null;

			var initializer = create.ChildNodes().FirstOrDefault(f => f is InitializerExpressionSyntax);

			if (initializer == null)
				return null;

			var r = new List<string>();

			foreach (var i in initializer.ChildNodes())
			{
				var expr = i as InitializerExpressionSyntax;

				if (expr == null)
					continue;

				if (expr.Expressions.Count == 0)
					continue;

				r.Add(expr.Expressions[0].GetText().ToString().Trim('"').ToLowerInvariant());
			}

			if (r.Count == 0)
				return null;

			return r;
		}
	}
}
