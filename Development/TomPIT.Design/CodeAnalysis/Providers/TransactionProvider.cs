using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class TransactionProvider : ComponentAnalysisProvider
	{
		public TransactionProvider(IExecutionContext context) : base(context)
		{

		}

		public override List<ICodeAnalysisResult> ProvideHover(IExecutionContext context, CodeAnalysisArgs e)
		{
			var transaction = context.Connection().GetService<IComponentService>().SelectComponent(e.Component.MicroService, ComponentCategory, e.ExpressionText);

			if (transaction == null)
				return null;

			if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(transaction.Token) is ITransaction config))
				return null;

			var r = new List<ICodeAnalysisResult>
			{
				new CodeAnalysisResult( ProviderUtils.Attribute(nameof(config.CommandText), config.CommandText)),
				new CodeAnalysisResult(ProviderUtils.Attribute(nameof(config.CommandType), config.CommandType.ToString())),
				new CodeAnalysisResult(ProviderUtils.Attribute(nameof(config.Connection), GetConnection(context, transaction, config.Connection)))
			};

			if (config.Parameters.Count > 0)
			{
				r.Add(new CodeAnalysisResult(ProviderUtils.Header(nameof(config.Parameters))));

				var pars = config.Parameters.OrderBy(f => f.Name);

				foreach (var i in pars)
				{
					if (i is IReturnValueParameter)
						r.Add(new CodeAnalysisResult(ProviderUtils.ListItem(string.Format("{0} ({1}, return)", i.Name, i.DataType))));
					else
					{
						if (i.IsNullable)
							r.Add(new CodeAnalysisResult(ProviderUtils.ListItem(string.Format("{0} ({1}, *null*)", i.Name, i.DataType))));
						else
							r.Add(new CodeAnalysisResult(ProviderUtils.ListItem(string.Format("{0} ({1})", i.Name, i.DataType))));
					}
				}
			}

			return r;
		}

		protected override string ComponentCategory => "Transaction";
	}
}
