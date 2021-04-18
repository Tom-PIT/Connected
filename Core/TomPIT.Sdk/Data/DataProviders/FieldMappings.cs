using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Models;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Data.DataProviders
{
	internal class FieldMappings<T>
	{
		private Dictionary<int, PropertyInfo> _properties;

		public FieldMappings(IMiddlewareContext context, IDataReader reader)
		{
			Context = context;
			Initialize(reader);
		}

		private Dictionary<int, PropertyInfo> Properties => _properties;
		private IMiddlewareContext Context { get; }

		private void Initialize(IDataReader reader)
		{
			if (typeof(T).IsTypePrimitive())
				return;

			_properties = new Dictionary<int, PropertyInfo>();

			var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			for (var i = 0; i < reader.FieldCount; i++)
			{
				if (ResolveProperty(properties, reader.GetName(i)) is PropertyInfo property)
					_properties.Add(i, property);
			}
		}

		public T CreateInstance(IDataReader reader)
		{
			if (typeof(T).IsTypePrimitive())
			{
				if (reader.FieldCount == 0)
					return default;

				return Types.Convert<T>(reader[0]);
			}

			var instance = typeof(T).CreateInstance();

			foreach (var property in Properties)
				SetValue(instance, property, reader);

			if (instance is IDataEntity entity)
				entity.Deserialize(Context, reader);

			return (T)instance;
		}

		private PropertyInfo ResolveProperty(PropertyInfo[] properties, string name)
		{
			if (properties.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0) is PropertyInfo property && property.FindAttribute<IgnoreAttribute>() is null && property.CanWrite)
				return property;

			foreach (var prop in properties)
			{
				if (prop.FindAttribute<NameAttribute>() is NameAttribute nameAttribute && string.Compare(nameAttribute.ColumnName, name, true) == 0 && prop.CanWrite)
					return prop;
			}

			return null;
		}

		private void SetValue(object instance, KeyValuePair<int, PropertyInfo> property, IDataReader reader)
		{
			var value = reader.GetValue(property.Key);

			if (value is null || Convert.IsDBNull(value))
				return;

			if (property.Value.PropertyType == typeof(string) && value is byte[] bv)
				value = ((Version)bv).ToString();
			else if (property.Value.PropertyType == typeof(DateTimeOffset))
			{
				var att = property.Value.FindAttribute<DateAttribute>();
				var kind = DateKind.DateTime;

				if (att != null)
					kind = att.Kind;

				switch (kind)
				{
					case DateKind.DateTime:
					case DateKind.DateTime2:
					case DateKind.SmallDateTime:
					case DateKind.Time:
						value = Context.Services.Globalization.FromUtc((DateTimeOffset)value);
						break;
				}
			}
			else if (property.Value.PropertyType == typeof(DateTime))
				value = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);

			property.Value.SetValue(instance, value);
		}
	}
}
