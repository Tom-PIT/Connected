using System.Collections.Generic;

namespace TomPIT.Data.DataProviders.Deployment
{
	public interface ITable : ISchema
	{
		List<ITableColumn> Columns { get; }
		List<ITableIndex> Indexes { get; }
	}
}
