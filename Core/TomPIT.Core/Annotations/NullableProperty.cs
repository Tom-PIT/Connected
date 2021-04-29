using System;
using TomPIT.Data;

namespace TomPIT.Annotations
{
	public class NullableProperty<T> : INullableProperty
	{
		public NullableProperty(T value) : this(value, MappingMode.Application)
		{

		}

		public NullableProperty(T value, MappingMode mode)
		{
			Value = value;
			Mode = mode;
		}

		public T Value { get; }

		public bool IsNull => NullMapper.IsNull(Value, Mode);
		public MappingMode Mode { get; } = MappingMode.Application;

		object INullableProperty.MappedValue
		{
			get
			{
				if (Value is null || Convert.IsDBNull(Value))
					return null;

				//if (Value.GetType().IsCollection())
				//{
				//	if(Value.GetType().IsArray)
				//	{
				//		IEnumerable en;

				//		en.Any();
				//	}
				//}

				return NullMapper.Map(Value, Mode);
			}
		}
	}
}