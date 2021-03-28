
using TomPIT.Data.Sql;

namespace TomPIT.Annotations
{
	public class NullableProperty<T>: INullableProperty
	{
		public NullableProperty(T value)
		{
			Value = value;
		}

		public T Value { get; }

		object INullableProperty.MappedValue => DatabaseCommand.MapNullValue(Value);
	}
}
