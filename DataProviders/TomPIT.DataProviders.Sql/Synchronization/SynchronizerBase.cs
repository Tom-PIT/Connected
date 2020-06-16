using System;
using System.Data;
using System.Data.SqlClient;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal abstract class SynchronizerBase
	{
		protected SynchronizerBase(SqlCommand command, IModelSchema schema)
		{
			Command = command;
			Schema = schema;
		}

		protected SqlCommand Command { get; }
		public IModelSchema Schema { get; }

		public void Execute()
		{
			OnExecute();
		}

		protected virtual void OnExecute()
		{

		}

		public string SchemaName => string.IsNullOrWhiteSpace(Schema.Schema) ? "dbo" : Schema.Schema;

		public string Qualifier(string value)
		{
			return $"[{Undress(value)}]";
		}

		public string Undress(string value)
		{
			return value.TrimStart('[').TrimEnd(']');
		}
		public string Qualifier(string schema, string name)
		{
			return $"{Qualifier(schema)}.{Qualifier(name)}";
		}

		public DbType ToDbType(string value)
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
				return DbType.Decimal;
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
				return DbType.Int64;
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

		public T GetValue<T>(SqlDataReader r, string fieldName, T defaultValue)
		{
			var idx = r.GetOrdinal(fieldName);

			if (idx == -1)
				return defaultValue;

			if (r.IsDBNull(idx))
				return defaultValue;

			return (T)Convert.ChangeType(r.GetValue(idx), typeof(T));
		}

		public string ResolveColumnLength(IModelSchemaColumn column)
		{
			if (column.MaxLength == -1)
				return "MAX";

			if (column.MaxLength > 0)
				return column.MaxLength.ToString();

			switch (column.DataType)
			{
				case DbType.AnsiString:
				case DbType.String:
				case DbType.Date:
				case DbType.DateTime:
				case DbType.AnsiStringFixedLength:
				case DbType.StringFixedLength:
					return 50.ToString();
				case DbType.Binary:
					return 128.ToString();
				case DbType.Time:
				case DbType.DateTime2:
				case DbType.DateTimeOffset:
					return 7.ToString();
				case DbType.VarNumeric:
					return 8.ToString();
				case DbType.Xml:
					return "MAX";
				default:
					return 50.ToString();
			}
		}

		public string CreateDataTypeMetaData(IModelSchemaColumn column)
		{
			return column.DataType switch
			{
				DbType.AnsiString => $"[varchar]({ResolveColumnLength(column)}",
				DbType.Binary => $"[binary]({ResolveColumnLength(column)}",
				DbType.Byte => "[tinyint]",
				DbType.Boolean => "[bit]",
				DbType.Currency => "[money]",
				DbType.Date => "[date]",
				DbType.DateTime => "[datetime]",
				DbType.Decimal => "[float]",
				DbType.Double => "[float]",
				DbType.Guid => "[uniqueidentifier]",
				DbType.Int16 => "[smallint]",
				DbType.Int32 => "[int]",
				DbType.Int64 => "[bigint]",
				DbType.Object => $"[varbinary]({ResolveColumnLength(column)})",
				DbType.SByte => "[smallint]",
				DbType.Single => "[float]",
				DbType.String => $"[nvarchar]({ResolveColumnLength(column)})",
				DbType.Time => $"[time]({ResolveColumnLength(column)})",
				DbType.UInt16 => "[int]",
				DbType.UInt32 => "[bigint]",
				DbType.UInt64 => "[float]",
				DbType.VarNumeric => "[float]",
				DbType.AnsiStringFixedLength => $"[char]({ResolveColumnLength(column)})",
				DbType.StringFixedLength => $"[char]({ResolveColumnLength(column)})",
				DbType.Xml => "[xml]",
				DbType.DateTime2 => $"[datetime2]({ResolveColumnLength(column)})",
				DbType.DateTimeOffset => $"[datetimeoffset]({ResolveColumnLength(column)})",
				_ => throw new NotSupportedException(),
			};
		}
	}
}
