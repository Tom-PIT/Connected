using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Models;
using TomPIT.Data.Schema;

namespace TomPIT.Data.Storage;
internal class SchemaColumn : IEquatable<ISchemaColumn>, ISchemaColumn
{
	public SchemaColumn(ISchema schema, PropertyInfo property)
	{
		Schema = schema;
		Property = property;
	}
	private ISchema Schema { get; }

	public string? Name { get; set; }
	public DbType DataType { get; set; }
	public bool IsIdentity { get; set; }
	public bool IsUnique { get; set; }
	public bool IsVersion { get; set; }
	public bool IsIndex { get; set; }
	public bool IsPrimaryKey { get; set; }
	public string? DefaultValue { get; set; }
	public int MaxLength { get; set; }
	public bool IsNullable { get; set; }
	public string? Index { get; set; }
	public int Precision { get; set; }
	public int Scale { get; set; }
	public DateKind DateKind { get; set; } = DateKind.DateTime;
	public BinaryKind BinaryKind { get; set; } = BinaryKind.VarBinary;
	public int DatePrecision { get; set; }

	public int Ordinal { get; set; }

	public PropertyInfo Property { get; set; }

	public bool Equals(ISchemaColumn? other)
	{
		if (other is null)
			return false;

		if (!string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase))
			return false;

		if (DataType != other.DataType)
			return false;

		if (IsIdentity != other.IsIdentity)
			return false;

		if (IsUnique != other.IsUnique)
			return false;

		if (IsIndex != other.IsIndex)
			return false;

		if (IsVersion != other.IsVersion)
			return false;

		if (IsPrimaryKey != other.IsPrimaryKey)
			return false;

		if (Precision != other.Precision)
			return false;

		if (Scale != other.Scale)
			return false;

		if (!string.Equals(DefaultValue, other.DefaultValue, StringComparison.Ordinal))
			return false;

		if (MaxLength != other.MaxLength)
			return false;

		if (IsNullable != other.IsNullable)
			return false;

		if (DateKind != other.DateKind)
			return false;

		if (DatePrecision != other.DatePrecision)
			return false;

		if (BinaryKind != other.BinaryKind)
			return false;

		if (other is IExistingSchemaColumn existing)
		{
			var existingColumns = existing.QueryIndexColumns(Name);

			if (existingColumns.Any() || IsIndex)
			{
				var columns = new List<string>();

				if (!string.IsNullOrWhiteSpace(Index))
				{
					foreach (var column in Schema.Columns)
					{
						if (string.Equals(column.Index, Index, StringComparison.OrdinalIgnoreCase))
							columns.Add(column.Name);
					}
				}
				else
					columns.Add(Name);

				if (existingColumns.Length != columns.Count)
					return false;

				existingColumns = existingColumns.Sort();
				columns.Sort();

				for (var i = 0; i < existingColumns.Length; i++)
				{
					if (!string.Equals(existingColumns[i], columns[i], StringComparison.OrdinalIgnoreCase))
						return false;
				}
			}
		}
		else
			return string.Equals(Index, other.Index, StringComparison.Ordinal);

		return true;
	}
}
