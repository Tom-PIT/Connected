using System.Collections.Generic;

namespace TomPIT.Data.Schema;
public interface ITableIndex
{
	string Name { get; }
	List<string> Columns { get; }
}
