using System;
using TomPIT.Annotations;
using TomPIT.Serialization;

namespace TomPIT.Data
{
	public enum MappingMode
	{
		Application = 1,
		Database = 2
	}
	public static class NullMapper
	{
		public static bool IsNull(object value, MappingMode mode)
		{
			var result = Map(value, mode);

			return mode switch
			{
				MappingMode.Application => result == null,
				MappingMode.Database => result == DBNull.Value,
				_ => throw new NotSupportedException(),
			};
		}
		public static object Map(object value, MappingMode mode)
		{
			if (value == null || value == DBNull.Value)
				return Null(mode);
			else if (value is string)
			{
				string v = Trim(value as string);

				if (string.IsNullOrWhiteSpace(v))
					return Null(mode);
			}
			else if (value is DateTime)
			{
				if ((DateTime)value == DateTime.MinValue)
					return Null(mode);
			}
			else if (value is DateTimeOffset offset)
			{
				if (offset == DateTimeOffset.MinValue)
					return Null(mode);

				return offset.UtcDateTime;
			}
			else if (value is int || value is float || value is double || value is short || value is byte || value is long || value is decimal)
			{
				if (Convert.ToDecimal(value) == decimal.Zero)
					return Null(mode);
			}
			else if (value is byte[])
			{
				if (value == null || ((byte[])value).Length == 0)
					return Null(mode); 
			}
			else if (value is Guid)
			{
				if ((Guid)value == Guid.Empty)
					return Null(mode);
			}
			else if (value is Enum)
			{
				if (Convert.ToDouble(value) == 0d)
					return Null(mode);

				return Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
			}
			else if (value is TimeSpan)
			{
				if ((TimeSpan)value == TimeSpan.Zero)
					return Null(mode);
			}
			else if (value is INullableProperty na)
			{
				return na.MappedValue;
			}
			else
				return Serializer.Serialize(value);

			return value;
		}

		private static object Null(MappingMode mode)
		{
			return mode switch
			{
				MappingMode.Application => null,
				MappingMode.Database => DBNull.Value,
				_ => throw new NotSupportedException(),
			};
		}

		public static string Trim(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			return value.Trim();
		}
	}
}
