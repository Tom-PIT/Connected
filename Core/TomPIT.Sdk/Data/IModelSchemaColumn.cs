using System.Data;
using TomPIT.Annotations.Models;

namespace TomPIT.Data
{
	public interface IModelSchemaColumn
	{
		string Name { get; }
		DbType DataType { get; }

		bool IsIdentity { get; }
		bool IsUnique { get; }
		bool IsIndex { get; }
		bool IsPrimaryKey { get; }
		bool IsVersion { get; }
		string DefaultValue { get; }
		int MaxLength { get; }
		bool IsNullable { get; }
		string DependencyType { get; }
		string DependencyProperty { get; }
		string IndexGroup { get; }
		public int Scale { get; }
		public int Precision { get; }
		public DateKind DateKind { get; }
		public int DatePrecision { get; }
	}
}
