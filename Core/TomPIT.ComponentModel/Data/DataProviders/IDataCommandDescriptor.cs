using System.Collections.Generic;
using System.Data;

namespace TomPIT.Data.DataProviders
{
	public interface IDataCommandDescriptor
	{
		int CommandTimeout { get; }
		string ConnectionString { get; }
		string CommandText { get; }
		CommandType CommandType { get; }

		List<ICommandParameter> Parameters { get; }
	}
}
