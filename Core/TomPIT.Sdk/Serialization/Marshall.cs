using System;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Reflection;

namespace TomPIT.Serialization
{
	public static class Marshall
	{
		public static R Convert<R>(object value)
		{
			if (value == null)
				return Types.Convert<R>(value);

			if (NeedsMarshalling(value, typeof(R)))
			{
				if (value is DataEntity de && typeof(R).ImplementsInterface<DataEntity>())
				{
					var rtype = typeof(R);
					var instance = rtype.IsValueType ? rtype.DefaultValue() as IDataEntity : rtype.CreateInstance() as IDataEntity;

					instance.Deserialize(de);

					return (R)instance;
				}
				else
					return Serializer.Deserialize<R>(Serializer.Serialize(value));
			}
			else if (typeof(R) == typeof(JArray))
			{
				if (value is JArray)
					return (R)value;

				return Types.Convert<R>(Serializer.Deserialize<JArray>(Serializer.Serialize(value)));
			}
			else if (typeof(R) == typeof(JObject))
			{
				if (value is JObject)
					return (R)value;

				return Types.Convert<R>(Serializer.Deserialize<JObject>(Serializer.Serialize(value)));
			}

			return Types.Convert<R>(value);
		}

		public static object Convert(object value, Type destinationType)
		{
			if (value == null)
				return Types.Convert(value, destinationType);

			if (NeedsMarshalling(value, destinationType))
			{
				if (value is DataEntity de && destinationType.ImplementsInterface<DataEntity>())
				{
					var rtype = destinationType;
					var instance = rtype.IsValueType ? rtype.DefaultValue() as IDataEntity : rtype.CreateInstance() as IDataEntity;

					instance.Deserialize(de);

					return instance;
				}
				else
					return Serializer.Deserialize(Serializer.Serialize(value), destinationType);
			}
			else if (destinationType == typeof(JArray))
			{
				if (value is JArray)
					return value;

				return Types.Convert(Serializer.Deserialize<JArray>(Serializer.Serialize(value)), destinationType);
			}
			else if (destinationType == typeof(JObject))
			{
				if (value is JObject)
					return value;

				return Types.Convert(Serializer.Deserialize<JObject>(Serializer.Serialize(value)), destinationType);
			}

			return Types.Convert(value, destinationType);
		}

		public static bool NeedsMarshalling(object instance, Type type)
		{
			if (type == null)
				return false;

			if (IsSubmission(instance, type))
				return true;

			if (type.IsGenericType && instance.GetType().IsGenericType)
			{
				var types = type.GetGenericArguments();
				var instanceTypes = instance.GetType().GetGenericArguments();

				for (var i = 0; i < types.Length; i++)
				{
					if (IsSubmission(instanceTypes[i], types[i]))
						return true;
				}
			}

			return false;
		}

		public static bool IsSubmission(object instance, Type type)
		{
			return IsSubmission(instance.GetType(), type);
		}

		public static bool IsSubmission(Type instanceType, Type type)
		{
			return (string.IsNullOrWhiteSpace(type.Namespace) && string.IsNullOrWhiteSpace(instanceType.Namespace))
				&& string.Compare(type.Assembly.FullName, instanceType.Assembly.FullName, false) != 0;
		}

	}
}
