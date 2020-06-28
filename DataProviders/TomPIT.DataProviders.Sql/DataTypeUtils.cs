using System.Data;

namespace TomPIT.DataProviders.Sql
{
	internal static class DataTypeUtils
	{
		public static DbType ToDbType(string value)
		{
			if (string.Compare(value, "bigint", true) == 0)
				return DbType.Int64;
			else if (string.Compare(value, "binary", true) == 0)
				return DbType.Binary;
			else if (string.Compare(value, "bit", true) == 0)
				return DbType.Boolean;
			else if (string.Compare(value, "char", true) == 0)
				return DbType.AnsiStringFixedLength;
			else if (string.Compare(value, "date", true) == 0)
				return DbType.Date;
			else if (string.Compare(value, "datetime", true) == 0)
				return DbType.DateTime;
			else if (string.Compare(value, "datetime2", true) == 0)
				return DbType.DateTime2;
			else if (string.Compare(value, "datetimeoffset", true) == 0)
				return DbType.DateTimeOffset;
			else if (string.Compare(value, "decimal", true) == 0)
				return DbType.Decimal;
			else if (string.Compare(value, "float", true) == 0)
				return DbType.Single;
			else if (string.Compare(value, "geography", true) == 0)
				return DbType.Object;
			else if (string.Compare(value, "hierarchyid", true) == 0)
				return DbType.Object;
			else if (string.Compare(value, "image", true) == 0)
				return DbType.Binary;
			else if (string.Compare(value, "int", true) == 0)
				return DbType.Int32;
			else if (string.Compare(value, "money", true) == 0)
				return DbType.Currency;
			else if (string.Compare(value, "nchar", true) == 0)
				return DbType.StringFixedLength;
			else if (string.Compare(value, "ntext", true) == 0)
				return DbType.String;
			else if (string.Compare(value, "numeric", true) == 0)
				return DbType.VarNumeric;
			else if (string.Compare(value, "nvarchar", true) == 0)
				return DbType.String;
			else if (string.Compare(value, "real", true) == 0)
				return DbType.Double;
			else if (string.Compare(value, "smalldatetime", true) == 0)
				return DbType.DateTime;
			else if (string.Compare(value, "smallmoney", true) == 0)
				return DbType.Currency;
			else if (string.Compare(value, "sql_variant", true) == 0)
				return DbType.Object;
			else if (string.Compare(value, "text", true) == 0)
				return DbType.String;
			else if (string.Compare(value, "time", true) == 0)
				return DbType.Time;
			else if (string.Compare(value, "timestamp", true) == 0)
				return DbType.Binary;
			else if (string.Compare(value, "tinyint", true) == 0)
				return DbType.Byte;
			else if (string.Compare(value, "uniqueidentifier", true) == 0)
				return DbType.Guid;
			else if (string.Compare(value, "varbinary", true) == 0)
				return DbType.Binary;
			else if (string.Compare(value, "varchar", true) == 0)
				return DbType.AnsiString;
			else if (string.Compare(value, "xml", true) == 0)
				return DbType.Xml;
			else
				return DbType.String;
		}
	}
}
