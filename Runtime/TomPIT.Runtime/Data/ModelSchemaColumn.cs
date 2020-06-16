using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace TomPIT.Data
{
	internal class ModelSchemaColumn : IEquatable<IModelSchemaColumn>, IModelSchemaColumn
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

		public bool Equals([AllowNull] IModelSchemaColumn other)
		{
			if (other == null)
				return false;

			if (string.Compare(Name, other.Name, true) != 0)
				return false;

			if (DataType != other.DataType)
				return false;

			if (IsIdentity != other.IsIdentity)
				return false;

			if (IsUnique != other.IsUnique)
				return false;

			if (IsIndex != other.IsIndex)
				return false;

			if (IsPrimaryKey != other.IsPrimaryKey)
				return false;

			if (string.Compare(DefaultValue, other.DefaultValue, false) != 0)
				return false;

			if (MaxLength != other.MaxLength)
				return false;

			if (IsNullable != other.IsNullable)
				return false;

			if (string.Compare(DependencyType, other.DependencyType, false) != 0)
				return false;

			if (string.Compare(DependencyProperty, other.DependencyProperty, false) != 0)
				return false;

			if (string.Compare(IndexGroup, other.IndexGroup, false) != 0)
				return false;

			return true;
		}
	}
}
