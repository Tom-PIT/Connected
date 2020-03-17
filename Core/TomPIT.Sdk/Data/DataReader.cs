using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.Data
{
	internal class DataReader<T> : DataCommand, IDataReader<T>
	{
		public DataReader(IMiddlewareContext context) : base(context)
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

					T instance = default;

					if (typeof(T).IsTypePrimitive())
					{
						if (row.Count == 0)
							return default;

						var property = row.First.Value<JProperty>();

						instance = Types.Convert<T>(property.Value);
					}
					else
						instance = Serializer.Deserialize<T>(Serializer.Serialize(record));

					if (instance is IDataEntity entity)
						entity.DataSource(row);

					r.Add(instance);
				}

				return r;
			}
			finally
			{
				if (Connection.Behavior == ConnectionBehavior.Isolated)
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

				if (typeof(T).IsTypePrimitive())
				{
					if (row.Count == 0)
						return default;

					var property = row.First.Value<JProperty>();

					return Types.Convert<T>(property.Value);
				}

				var instance = Serializer.Deserialize<T>(Serializer.Serialize(row));

				if (instance is IDataEntity entity)
					entity.DataSource(row);

				return instance;
			}
			finally
			{
				if (Connection.Behavior == ConnectionBehavior.Shared)
					Connection.Close();
			}
		}
	}
}
