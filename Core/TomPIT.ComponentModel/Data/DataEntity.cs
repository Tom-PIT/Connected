using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;

namespace TomPIT.Data
{
	public abstract class DataEntity : IDataEntity
	{
		public string Serialize()
		{
			return OnSerialize();
		}

		protected virtual string OnSerialize()
		{
			return Types.Serialize(this);
		}
		public void Deserialize(JObject state)
		{
			OnDeserialize(state);
			OnDeserialized();
		}

		protected virtual void OnDeserialize(JObject state)
		{
			var settings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			};

			JsonConvert.PopulateObject(Types.Serialize(state), this, settings);
		}

		protected virtual void OnDeserialized()
		{

		}

		public void DataSource(JObject state)
		{
			OnDataSource(state);
		}

		protected virtual void OnDataSource(JObject state)
		{

		}

		public T Evolve<T>() where T : class, IDataEntity
		{
			var instance = typeof(T).CreateInstance<T>();

			instance.Deserialize(this);

			return instance;
		}

		public static implicit operator JObject(DataEntity entity)
		{
			return Types.Deserialize<JObject>(entity.Serialize());
		}
	}
}