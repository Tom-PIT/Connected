using System.Collections.Generic;

namespace TomPIT.Data.Schema;
public interface IDatabase
{
	List<ITable> Tables { get; }
}
