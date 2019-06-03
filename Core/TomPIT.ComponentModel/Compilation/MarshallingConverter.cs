using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Data;

namespace TomPIT.Compilation
{
	public static class MarshallingConverter
	{
		public static R Convert<R>(object value)
		{
			if(value == null)
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
				{
					var settings = new JsonSerializerSettings
					{
						DefaultValueHandling = DefaultValueHandling.Ignore,
						TypeNameHandling = TypeNameHandling.None,
						ContractResolver = new SerializationResolver()
					};

					return JsonConvert.DeserializeObject<R>(JsonConvert.SerializeObject(value, settings), settings);
				}
			}
			else if (typeof(R) == typeof(JArray))
			{
				if (value is JArray)
					return (R)value;

				return Types.Convert<R>(JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(value)));
			}
			else if (typeof(R) == typeof(JObject))
			{
				if (value is JObject)
					return (R)value;

				return Types.Convert<R>(JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(value)));
			}

			return Types.Convert<R>(value);
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
			return (string.IsNullOrWhiteSpace(type.Namespace) && string.IsNullOrWhiteSpace(instance.GetType().Namespace))
				&& string.Compare(type.Assembly.FullName, instance.GetType().Assembly.FullName, false) != 0;
		}

		public static bool IsSubmission(Type instanceType, Type type)
		{
			return (string.IsNullOrWhiteSpace(type.Namespace) && string.IsNullOrWhiteSpace(instanceType.Namespace))
				&& string.Compare(type.Assembly.FullName, instanceType.Assembly.FullName, false) != 0;
		}
	}
}
