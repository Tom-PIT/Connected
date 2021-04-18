using TomPIT.Reflection;

namespace TomPIT.Serialization
{
	public class TypedSerializationDescriptor
	{
		public TypedSerializationDescriptor()
		{

		}
		public TypedSerializationDescriptor(object value)
		{
			if (value == null)
				return;

			Type = $"{value.GetType().FullName}, {value.GetType().Assembly.ShortName()}";
			Value = Serializer.Serialize(value);
		}

		public T Deserialize<T>()
		{
			return (T)Deserialize();
		}

		public object Deserialize()
		{
			var type = TypeExtensions.GetType(Type);

			if (type == null)
				return null;

			return Serializer.Deserialize(Value, type);
		}

		public static TypedSerializationDescriptor Create(string value)
		{
			return Serializer.Deserialize<TypedSerializationDescriptor>(value);
		}

		public string Type { get; set; }
		public string Value { get; set; }
	}
}
