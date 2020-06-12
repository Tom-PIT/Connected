using System.Data;

namespace TomPIT.Data
{
	public interface IModelSchemaColumn
	{
		string Name { get; set; }
		DbType DataType { get; }

		bool IsIdentity { get; }
		bool IsUnique { get; }
		bool IsIndex { get; }
		bool IsPrimaryKey { get; }
		string DefaultValue { get; }
		int MaxLength { get; }
		bool IsNullable { get; }
		string Dependency { get; }

	}
}
