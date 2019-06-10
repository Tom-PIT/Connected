using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Apis
{
	internal class DataReader<T> : DataCommand, IDataReader<T>
	{
		public DataReader(IExecutionContext context) : base(context)
		{
		}

		public List<T> Query()
		{
			try
			{
				var ds = Connection.Query(CreateCommand());
				var r = new List<T>();

				if (ds == null || ds.Count == 0)
					return r;

				var array = ds.Optional<JArray>("data", null);

				if (array == null)
					return r;

				foreach (var record in array)
				{
					if (!(record is JObject row))
						continue;

					var instance = Types.Deserialize<T>(Types.Serialize(record));

					r.Add(instance);
				}

				return r;
			}
			finally
			{
				if (CloseConnection)
					Connection.Close();
			}
		}

		public T Select()
		{
			try
			{
				var ds = Connection.Query(CreateCommand());

				if (ds == null || ds.Count == 0)
					return default;

				var array = ds.Optional<JArray>("data", null);

				if (array == null || array.Count == 0)
					return default;

				if (!(array[0] is JObject row))
					return default;

				return Types.Deserialize<T>(Types.Serialize(row));
			}
			finally
			{
				if (CloseConnection)
					Connection.Close();
			}
		}
	}
}