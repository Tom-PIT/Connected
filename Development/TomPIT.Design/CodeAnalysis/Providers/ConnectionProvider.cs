using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class ConnectionProvider : ComponentAnalysisProvider
	{
		public ConnectionProvider(IExecutionContext context) : base(context)
		{
		}

		protected override string ComponentCategory => "Connection";
		protected override bool FullyQualified => true;
		public override List<ICodeAnalysisResult> ProvideHover(IExecutionContext context, CodeAnalysisArgs e)
		{
			var connection = context.Connection().GetService<IComponentService>().SelectComponent(e.Component.MicroService, ComponentCategory, e.ExpressionText);

			if (connection == null)
				return null;

			if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(connection.Token) is IConnection config))
				return null;

			var provider = context.Connection().GetService<IDataProviderService>().Select(config.DataProvider);

			var r = new List<ICodeAnalysisResult>
			{
				new CodeAnalysisResult( ProviderUtils.Attribute( nameof(config.Value), config.Value), null,null),
				new CodeAnalysisResult( ProviderUtils.Attribute(nameof(config.DataProvider), provider == null?"?": provider.Name), null, null)
			};

			return r;
		}
	}
}
