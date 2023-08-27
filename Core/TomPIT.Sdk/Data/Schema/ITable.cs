using System.Collections.Generic;

namespace TomPIT.Data.Schema;
public interface ITable : ISchema
{
	List<ITableColumn> Columns { get; }
	List<ITableIndex> Indexes { get; }
}
