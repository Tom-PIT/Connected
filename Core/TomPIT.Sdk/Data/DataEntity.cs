﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.Reflection;
using TomPIT.Serialization;

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
			return Serializer.Serialize(this);
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

			JsonConvert.PopulateObject(Serializer.Serialize(state), this, settings);
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

				var atts = property.FindAttributes<MappingAttribute>();

				if (atts == null || atts.Count == 0)
					continue;

				foreach (var att in atts)
				{
					if (string.IsNullOrWhiteSpace(att.DataSourceField))
						continue;

					var prop = state.Property(att.DataSourceField, StringComparison.OrdinalIgnoreCase);

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

		public static implicit operator JObject(DataEntity entity)
		{
			return Serializer.Deserialize<JObject>(entity.Serialize());
		}
	}
}