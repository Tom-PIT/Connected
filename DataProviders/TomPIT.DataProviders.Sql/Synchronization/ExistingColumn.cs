using System.Data;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class ExistingColumn : IModelSchemaColumn
	{
		public string Name { get; set; }

		public DbType DataType { get; set; }

		public bool IsIdentity { get; set; }

		public bool IsUnique { get; set; }

		public bool IsIndex { get; set; }

		public bool IsPrimaryKey { get; set; }

		public string DefaultValue { get; set; }

		public int MaxLength { get; set; }

		public bool IsNullable { get; set; }

		public string DependencyType { get; set; }

		public string DependencyProperty { get; set; }

		public string IndexGroup { get; set; }
	}
}
