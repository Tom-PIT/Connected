using Newtonsoft.Json.Linq;

namespace TomPIT.Data
{
	public abstract class JsonEntity
	{
		public JsonEntity(JObject data)
		{
			OnDatabind();
		}

		protected JObject Data { get; }

		protected T GetValue<T>(string propertyName)
		{
			return GetValue(propertyName, default(T));
		}

		protected T GetValue<T>(string propertyName, T defaultValue)
		{
			var prop = Data.Property(propertyName);

			if (prop == null)
				return defaultValue;

			if (!(prop.Value is JValue v))
				return defaultValue;

			if (Types.TryConvert<T>(v.Value, out T r))
				return r;

			return defaultValue;
		}

		protected void SetValue<T>(string propertyName, T value)
		{
			var property = Data.Property(propertyName);

			if (property == null)
			{
				property = new JProperty(propertyName, value);

				Data.Add(property);
			}
			else
				property.Value = new JValue(value);
		}

		protected virtual void OnDatabind()
		{

		}
	}
}
