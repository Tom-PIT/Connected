using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TomPIT.Serialization
{
	public static class Serializer
	{
		private static JsonSerializerSettings _jsonSettings = null;
		private static JsonMergeSettings _mergeSettings = null;

		private static JsonSerializerSettings SerializerSettings
		{
			get
			{
				if (_jsonSettings == null)
				{
					_jsonSettings = new JsonSerializerSettings
					{
						Culture = CultureInfo.InvariantCulture,
						DateFormatHandling = DateFormatHandling.IsoDateFormat,
						DateParseHandling = DateParseHandling.DateTime,
						DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
						FloatFormatHandling = FloatFormatHandling.DefaultValue,
						FloatParseHandling = FloatParseHandling.Double,
						MissingMemberHandling = MissingMemberHandling.Ignore,
						Formatting = Formatting.Indented,
						DefaultValueHandling = DefaultValueHandling.Include,
						TypeNameHandling = TypeNameHandling.None,
						ContractResolver = new SerializationResolver(),
						NullValueHandling = NullValueHandling.Ignore,
						ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
						MetadataPropertyHandling = MetadataPropertyHandling.Default
					};
				}

				return _jsonSettings;
			}
		}

		//private static JsonSerializerSettings IgnoreMetaDataSerializerSettings
		//{
		//	get
		//	{
		//		if (_ignoreMetaDataJsonSettings == null)
		//		{
		//			_ignoreMetaDataJsonSettings = new JsonSerializerSettings
		//			{
		//				Culture = CultureInfo.InvariantCulture,
		//				DateFormatHandling = DateFormatHandling.IsoDateFormat,
		//				DateParseHandling = DateParseHandling.DateTime,
		//				DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
		//				FloatFormatHandling = FloatFormatHandling.DefaultValue,
		//				FloatParseHandling = FloatParseHandling.Double,
		//				MissingMemberHandling = MissingMemberHandling.Ignore,
		//				Formatting = Formatting.Indented,
		//				DefaultValueHandling = DefaultValueHandling.Include,
		//				TypeNameHandling = TypeNameHandling.Auto,
		//				ContractResolver = new SerializationResolver(),
		//				NullValueHandling = NullValueHandling.Ignore,
		//				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
		//				MetadataPropertyHandling = MetadataPropertyHandling.Ignore
		//			};
		//		}

		//		return _ignoreMetaDataJsonSettings;
		//	}
		//}

		private static JsonMergeSettings MergeSettings
		{
			get
			{
				if (_mergeSettings == null)
				{
					_mergeSettings = new JsonMergeSettings
					{
						MergeArrayHandling = MergeArrayHandling.Merge,
						MergeNullValueHandling = MergeNullValueHandling.Ignore,
						PropertyNameComparison = StringComparison.OrdinalIgnoreCase
					};
				}

				return _mergeSettings;
			}
		}

		public static void Merge(JObject left, object right)
		{
			if (left == null || right == null)
				return;

			left.Merge(right, MergeSettings);
		}

		public static string Serialize(object instance)
		{
			return JsonConvert.SerializeObject(instance, SerializerSettings);
		}

		public static void Populate(string value, object instance)
		{
			if (string.IsNullOrWhiteSpace(value))
				return;

			JsonConvert.PopulateObject(value, instance, SerializerSettings);
		}

		public static void Populate(object value, object instance)
		{
			if (value == null)
				return;

			JsonConvert.PopulateObject(Serialize(value), instance, SerializerSettings);
		}

		public static object Deserialize(string json, Type type)
		{
			if (string.IsNullOrWhiteSpace(json))
				return default;

			return JsonConvert.DeserializeObject(json, type, SerializerSettings);
		}

		public static T Deserialize<T>(object value)
		{
			if (value == null)
				return default;

			return value is string
				? Deserialize<T>(value.ToString())
				: Deserialize<T>(Serialize(value));
		}

		public static T Deserialize<T>(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
				return default;

			return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
		}
	}
}
