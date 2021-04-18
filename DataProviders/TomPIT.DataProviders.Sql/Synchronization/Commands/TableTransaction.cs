using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TomPIT.Annotations.Models;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal abstract class TableTransaction : SynchronizationTransaction
	{
		public TableTransaction(ISynchronizer owner) : base(owner)
		{
		}

		protected string CreateColumnCommandText(IModelSchemaColumn column)
		{
			var builder = new StringBuilder();

			builder.AppendFormat($"{Escape(column.Name)} {CreateDataTypeMetaData(column)} ");

			if (column.IsIdentity)
				builder.Append("IDENTITY(1,1) ");

			if (column.IsNullable)
				builder.Append("NULL ");
			else
				builder.Append("NOT NULL ");

			return builder.ToString();
		}

		protected string ResolveColumnLength(IModelSchemaColumn column)
		{
			if (column.MaxLength == -1)
				return "MAX";

			if (column.MaxLength > 0)
				return column.MaxLength.ToString();

			switch (column.DataType)
			{
				case DbType.AnsiString:
				case DbType.String:
				case DbType.AnsiStringFixedLength:
				case DbType.StringFixedLength:
					return 50.ToString();
				case DbType.Binary:
					return 128.ToString();
				case DbType.Time:
				case DbType.DateTime2:
				case DbType.DateTimeOffset:
					return column.DatePrecision.ToString();
				case DbType.VarNumeric:
					return 8.ToString();
				case DbType.Xml:
					return "MAX";
				case DbType.Decimal:
					return $"{column.Precision}, {column.Scale}";
				default:
					return 50.ToString();
			}
		}

		protected string CreateDataTypeMetaData(IModelSchemaColumn column)
		{
			return column.DataType switch
			{
				DbType.AnsiString => $"[varchar]({ResolveColumnLength(column)})",
				DbType.Binary => column.IsVersion ? "[timestamp]" : column.BinaryKind == BinaryKind.Binary ? $"[binary]({ResolveColumnLength(column)})" : $"[varbinary]({ResolveColumnLength(column)})",
				DbType.Byte => "[tinyint]",
				DbType.Boolean => "[bit]",
				DbType.Currency => "[money]",
				DbType.Date => "[date]",
				DbType.DateTime => column.DateKind == DateKind.SmallDateTime ? "[smalldatetime]" : "[datetime]",
				DbType.Decimal => $"[decimal]({ResolveColumnLength(column)})",
				DbType.Double => "[real]",
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
				DbType.VarNumeric => $"[numeric]({ResolveColumnLength(column)})",
				DbType.AnsiStringFixedLength => $"[char]({ResolveColumnLength(column)})",
				DbType.StringFixedLength => $"[nchar]({ResolveColumnLength(column)})",
				DbType.Xml => "[xml]",
				DbType.DateTime2 => $"[datetime2]({ResolveColumnLength(column)})",
				DbType.DateTimeOffset => $"[datetimeoffset]({ResolveColumnLength(column)})",
				_ => throw new NotSupportedException(),
			};
		}

		protected List<IndexDescriptor> ParseIndexes(IModelSchema schema)
		{
			var result = new List<IndexDescriptor>();

			foreach (var column in schema.Columns)
			{
				if (column.IsPrimaryKey)
					continue;

				if (column.IsIndex)
				{
					if (string.IsNullOrWhiteSpace(column.IndexGroup))
					{
						var index = new IndexDescriptor
						{
							Unique = column.IsUnique,
						};

						index.Columns.Add(column.Name);

						result.Add(index);
					}
					else
					{
						var index = result.FirstOrDefault(f => string.Compare(f.Group, column.IndexGroup, true) == 0);

						if (index == null)
						{
							index = new IndexDescriptor
							{
								Group = column.IndexGroup,
								Unique = column.IsUnique
							};

							result.Add(index);
						}

						index.Columns.Add(column.Name);
					}
				}
			}

			return result;
		}
	}
}
