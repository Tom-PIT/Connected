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
		[Obsolete]
		protected virtual T GetValue<T>(string propertyName)
		{
			return default;
		}
		[Obsolete]
		protected virtual T GetValue<T>(string propertyName, T defaultValue)
		{
			return defaultValue;
		}
		[Obsolete]
		protected virtual void SetValue<T>(string propertyName, T value)
		{

		}

		protected virtual void OnDatabind()
		{

		}

		public string Serialize()
		{
			return OnSerialize();
		}

		protected virtual string OnSerialize()
		{
			return JsonConvert.SerializeObject(this);
		}
		public void Deserialize(JObject state)
		{
			OnDeserialize(state);
		}

		protected virtual void OnDeserialize(JObject state)
		{
			JsonConvert.PopulateObject(JsonConvert.SerializeObject(state), this);
		}

		public static implicit operator JObject(DataEntity entity)
		{
			return JsonConvert.DeserializeObject<JObject>(entity.Serialize());
		}
	}
}