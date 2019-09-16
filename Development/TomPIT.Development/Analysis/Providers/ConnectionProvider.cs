using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class ConnectionProvider : ComponentAnalysisProvider
	{
		public ConnectionProvider(IMiddlewareContext context) : base(context)
		{
		}

		protected override string ComponentCategory => ComponentCategories.Connection;
		protected override bool FullyQualified => true;
		public override List<ICodeAnalysisResult> ProvideHover(IMiddlewareContext context, CodeAnalysisArgs e)
		{
			var connection = context.Tenant.GetService<IComponentService>().SelectComponent(e.Component.MicroService, ComponentCategory, e.ExpressionText);

			if (connection == null)
				return null;

			if (!(context.Tenant.GetService<IComponentService>().SelectConfiguration(connection.Token) is IConnectionConfiguration config))
				return null;

			var provider = context.Tenant.GetService<IDataProviderService>().Select(config.DataProvider);

			var r = new List<ICodeAnalysisResult>
			{
				new CodeAnalysisResult( ProviderUtils.Attribute( nameof(config.Value), config.Value), null,null),
				new CodeAnalysisResult( ProviderUtils.Attribute(nameof(config.DataProvider), provider == null?"?": provider.Name), null, null)
			};

			return r;
		}
	}
}
