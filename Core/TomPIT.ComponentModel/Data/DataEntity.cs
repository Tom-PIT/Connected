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
			return JsonConvert.SerializeObject(this);
		}
		public void Deserialize(JObject state)
		{
			OnDeserialize(state);
		}

		protected virtual void OnDeserialize(JObject state)
		{
			var settings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			};

			JsonConvert.PopulateObject(JsonConvert.SerializeObject(state), this, settings);
		}

		public static implicit operator JObject(DataEntity entity)
		{
			return JsonConvert.DeserializeObject<JObject>(entity.Serialize());
		}
	}
}