using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Services;

namespace TomPIT.Design
{
	internal class DataSourceProvider : ComponentAnalysisProvider
	{
		public DataSourceProvider(IExecutionContext context) : base(context)
		{

		}

		public override List<ICodeAnalysisResult> ProvideHover(IExecutionContext context, CodeAnalysisArgs e)
		{
			var dataSource = context.Connection().GetService<IComponentService>().SelectComponent(e.Component.MicroService, ComponentCategory, e.ExpressionText);

			if (dataSource == null)
				return null;

			if (!(context.Connection().GetService<IComponentService>().SelectConfiguration(dataSource.Token) is IDataSource config))
				return null;

			var r = new List<ICodeAnalysisResult>
			{
				new CodeAnalysisResult( ProviderUtils.Attribute(nameof(config.CommandText), config.CommandText),null,null),
				new CodeAnalysisResult( ProviderUtils.Attribute(nameof(config.CommandType), config.CommandType.ToString()),null,null),
				new CodeAnalysisResult( ProviderUtils.Attribute(nameof(config.Connection), GetConnection(context, dataSource, config.Connection)),null,null)
			};

			if (config.Parameters.Count > 0)
			{
				r.Add(new CodeAnalysisResult(ProviderUtils.Header(nameof(config.Parameters)), null, null));

				var pars = config.Parameters.OrderBy(f => f.Name);

				foreach (var i in pars)
				{
					if (i.IsNullable)
						r.Add(new CodeAnalysisResult(ProviderUtils.ListItem(string.Format("{0} ({1}, *null*)", i.Name, i.DataType)), null, null));
					else
						r.Add(new CodeAnalysisResult(ProviderUtils.ListItem(string.Format("{0} ({1})", i.Name, i.DataType)), null, null));
				}
			}

			if (config.Fields.Count > 0)
			{
				r.Add(new CodeAnalysisResult(ProviderUtils.Header(nameof(config.Fields)), null, null));

				var fields = config.Fields.OrderBy(f => f.Name);

				foreach (var i in fields)
					r.Add(new CodeAnalysisResult(ProviderUtils.ListItem(string.Format("{0} ({1})", i.Name, i.DataType)), null, null));
			}


			return r;
		}

		protected override string ComponentCategory => "DataSource";
	}
}
