using System.Collections.Generic;

namespace TomPIT.Deployment.Database
{
	public interface ITable : ISchema
	{
		List<ITableColumn> Columns { get; }
		List<ITableIndex> Indexes { get; }
	}
}
