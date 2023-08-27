using System.Data;
using System.Reflection;
using TomPIT.Annotations.Models;

namespace TomPIT.Data.Schema;
public interface ISchemaColumn
{
	string? Name { get; }
	DbType DataType { get; }
	bool IsIdentity { get; }
	bool IsUnique { get; }
	bool IsIndex { get; }
	bool IsPrimaryKey { get; }
	bool IsVersion { get; }
	string? DefaultValue { get; }
	int MaxLength { get; }
	bool IsNullable { get; }
	string? Index { get; }
	int Scale { get; }
	int Precision { get; }
	DateKind DateKind { get; }
	BinaryKind BinaryKind { get; }
	int DatePrecision { get; }

	PropertyInfo Property { get; }
}
