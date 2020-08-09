using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Models;
using TomPIT.Exceptions;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.Data
{
	public abstract class DataEntity : IDataEntity
	{
		private static readonly JsonSerializerSettings _deserializeSettings;

		static DataEntity()
		{
			_deserializeSettings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new PrivateContractResolver()
			};
		}
		public string Serialize()
		{
			return OnSerialize();
		}

		protected virtual string OnSerialize()
		{
			return Serializer.Serialize(this);
		}
		public void Deserialize(JObject state)
		{
			OnDeserialize(state);
			OnDeserialized();
		}

		protected virtual void OnDeserialize(JObject state)
		{
			JsonConvert.PopulateObject(Serializer.Serialize(state), this, _deserializeSettings);
		}

		protected virtual void OnDeserialized()
		{

		}

		public void DataSource(JObject state)
		{
			var properties = GetType().GetProperties();

			foreach (var property in properties)
			{
				if (!property.CanWrite)
					continue;

				var atts = property.FindAttributes<NameAttribute>();

				if (atts == null || atts.Count == 0)
					continue;

				foreach (var att in atts)
				{
					if (string.IsNullOrWhiteSpace(att.ColumnName))
						continue;

					var prop = state.Property(att.ColumnName, StringComparison.OrdinalIgnoreCase);

					if (prop == null)
						continue;

					property.SetValue(this, Types.Convert(((JValue)prop.Value).Value, property.PropertyType));
				}
			}

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

		public static void MergeProperties(object instance, Dictionary<string, object> properties)
		{
			if (properties == null || properties.Count == 0)
				return;

			foreach (var property in properties)
			{
				var reflected = instance.GetType().GetProperty(property.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				if (reflected == null)
					throw new RuntimeException($"{SR.ErrPropertyNotFound} ({property.Key})");

				if (!reflected.CanWrite)
					throw new RuntimeException($"SR.ErrPropertyReadOnly ({property.Key})");

				var value = Types.Convert(property.Value, reflected.PropertyType);

				reflected.SetValue(instance, value);
			}
		}

		public void Merge(Dictionary<string, object> properties)
		{
			MergeProperties(this, properties);
		}

		public static implicit operator JObject(DataEntity entity)
		{
			return Serializer.Deserialize<JObject>(entity.Serialize());
		}
	}
}