using System;
using System.Collections.Generic;
using System.Data;

namespace TomPIT.Data
{
	public interface IDataCommand : IDisposable
	{
		string CommandText { get; set; }
		CommandType CommandType { get; set; }
		int CommandTimeout { get; set; }
		IDataConnection Connection { get; set; }

		List<IDataParameter> Parameters { get; }

		IDataParameter SetParameter(string name, object value);
		IDataParameter SetParameter(string name, object value, bool nullMapping);
		IDataParameter SetParameter(string name, object value, bool nullMapping, DbType type);
	}
}
