using Newtonsoft.Json.Linq;
using System;

namespace TomPIT.Data
{
	public abstract class JsonEntity:DataEntity
	{
        protected JsonEntity() { Data = new JObject(); }
		protected JsonEntity(JObject data)
		{
			if (data == null)
				data = new JObject();

			Data = data;

			OnDatabind();
		}

		protected JObject Data { get; }

		protected override T GetValue<T>(string propertyName)
		{
			return GetValue(propertyName, default(T));
		}

		protected override T GetValue<T>(string propertyName, T defaultValue)
		{
			var prop = Data.Property(propertyName, StringComparison.OrdinalIgnoreCase);

			if (prop == null)
				return defaultValue;

			if (!(prop.Value is JValue v))
				return defaultValue;

			if (Types.TryConvert<T>(v.Value, out T r))
				return r;

			return defaultValue;
		}

		protected override void SetValue<T>(string propertyName, T value)
		{
			var property = Data.Property(propertyName, StringComparison.OrdinalIgnoreCase);

			if (property == null)
			{
				property = new JProperty(propertyName, value);

				Data.Add(property);
			}
			else
				property.Value = new JValue(value);
		}
	}
}
