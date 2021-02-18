using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Models;
using TomPIT.Middleware;
using TomPIT.Reflection;

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
				EnsureCommand();
				var ds = Connection.Query(Command);
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

					T instance;

					if (typeof(T).IsTypePrimitive())
					{
						if (row.Count == 0)
							return default;

						var property = row.First.Value<JProperty>();

						instance = Types.Convert<T>(property.Value);
					}
					else
						instance = CreateRecord(row);

					r.Add(instance);
				}

				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Commit();

				return r;
			}
			finally
			{
				if (Connection.Behavior == ConnectionBehavior.Isolated)
				{
					Connection.Close();
					Connection.Dispose();
					Connection = null;
				}
			}
		}

		public T Select()
		{
			try
			{
				EnsureCommand();
				var ds = Connection.Query(Command);

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

				var instance = CreateRecord(row);

				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Commit();

				return instance;
			}
			finally
			{
				if (Connection.Behavior == ConnectionBehavior.Isolated)
				{
					Connection.Close();
					Connection.Dispose();
					Connection = null;
				}
			}
		}

		private T CreateRecord(JObject row)
		{
			var instance = typeof(T).CreateInstance();

			Deserialize(instance, row);

			if (instance is IDataEntity entity)
				entity.DataSource(row);

			return (T)instance;
		}

		private void Deserialize(object instance, JObject row)
		{
			var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var property in properties)
			{
				var ignore = property.FindAttribute<IgnoreAttribute>();

				if (ignore != null)
					continue;

				var att = property.FindAttribute<NameAttribute>();
				var name = property.Name;

				if (att != null)
					name = att.ColumnName;

				var rowProperty = row.Property(name, StringComparison.OrdinalIgnoreCase);

				if (rowProperty == null)
					continue;

				var value = rowProperty.Value;

				if (property.IsPrimitive())
					SetValue(instance, property, value);
				else if (property.PropertyType.IsCollection())
					DeserializeCollection(instance, property, value);
				else
					DeserializeObject(instance, property, value);
			}
		}

		private void DeserializeCollection(object instance, PropertyInfo property, JToken token)
		{
			if (property.PropertyType == typeof(byte[]))
			{
				SetValue(instance, property, token);
				return;
			}

			var currentValue = EnsureInstance(instance, property) as IList;

			if (currentValue == null)
				return;

			if (!(token is JArray array))
				return;

			foreach (var item in array)
			{
				var arrayType = currentValue.GetType().GetElementType();

				if (item is JValue value)
				{
					if (!arrayType.IsPrimitive)
						continue;

					var converted = ConvertValue(arrayType, value.Value);

					if (converted != null)
						currentValue.Add(converted);
				}
				else if (item is JObject obj)
				{
					var arrayItem = arrayType.CreateInstance();

					currentValue.Add(arrayItem);

					Deserialize(arrayItem, obj);
				}
			}
		}

		private void DeserializeObject(object instance, PropertyInfo property, JToken token)
		{
			var currentValue = EnsureInstance(instance, property);

			if (currentValue == null)
				return;

			if (!(token is JObject o))
				return;

			Deserialize(currentValue, o);
		}

		private void SetValue(object instance, PropertyInfo property, JToken token)
		{
			if (!property.CanWrite)
				return;

			if (!(token is JValue jv))
				return;

			object converted = null;

			if (property.PropertyType == typeof(string) && jv.Value is byte[])
				converted = ((Version)(byte[])jv.Value).ToString();
			else
				converted = ConvertValue(property.PropertyType, jv.Value);

			if (converted == null)
				return;

			property.SetValue(instance, converted);
		}

		private object EnsureInstance(object instance, PropertyInfo property)
		{
			var currentValue = property.GetValue(instance);

			if (currentValue != null)
				return currentValue;

			if (!property.CanWrite)
				return null;

			property.SetValue(instance, property.PropertyType.CreateInstance());

			return property.GetValue(instance);
		}

		private object ConvertValue(Type propertyType, object value)
		{
			if (value == null)
				return value;

			var converter = TypeDescriptor.GetConverter(propertyType);

			if (!converter.CanConvertFrom(value.GetType()))
			{
				if (Types.TryConvert(value, out object result, propertyType))
					return result;

				return null;
			}

			return converter.ConvertFrom(value);
		}
	}
}
