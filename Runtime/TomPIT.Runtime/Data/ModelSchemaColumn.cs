using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using TomPIT.Annotations.Models;

namespace TomPIT.Data
{
	internal class ModelSchemaColumn : IEquatable<IModelSchemaColumn>, IModelSchemaColumn
	{
		public ModelSchemaColumn(IModelSchema schema)
		{
			Schema = schema;
		}
		private IModelSchema Schema { get; }

		public string Name { get; set; }
		public DbType DataType { get; set; }

		public bool IsIdentity { get; set; }
		public bool IsUnique { get; set; }
		public bool IsVersion { get; set; }
		public bool IsIndex { get; set; }
		public bool IsPrimaryKey { get; set; }
		public string DefaultValue { get; set; }
		public int MaxLength { get; set; }
		public bool IsNullable { get; set; }
		public string DependencyType { get; set; }
		public string DependencyProperty { get; set; }
		public string IndexGroup { get; set; }
		public int Precision { get; set; }
		public int Scale { get; set; }
		public DateKind DateKind { get; set; }
		public int DatePrecision { get; set; }

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

			if (IsVersion != other.IsVersion)
				return false;

			if (IsPrimaryKey != other.IsPrimaryKey)
				return false;

			if (Precision != other.Precision)
				return false;

			if (Scale != other.Scale)
				return false;

			if (string.Compare(DefaultValue, other.DefaultValue, false) != 0)
				return false;

			if (MaxLength != other.MaxLength)
				return false;

			if (IsNullable != other.IsNullable)
				return false;

			if (DateKind != other.DateKind)
				return false;

			if (DatePrecision != other.DatePrecision)
				return false;

			if (other is IExistingModelSchemaColumn existing)
			{
				var existingColumns = existing.QueryIndexColumns(Name);

				if (existingColumns.Count > 0 || IsIndex)
				{
					var columns = new List<string>();

					if (!string.IsNullOrWhiteSpace(IndexGroup))
					{
						foreach (var column in Schema.Columns)
						{
							if (string.Compare(column.IndexGroup, IndexGroup, true) == 0)
								columns.Add(column.Name);
						}
					}
					else
						columns.Add(Name);

					if (existingColumns.Count != columns.Count)
						return false;

					existingColumns.Sort();
					columns.Sort();

					for (var i = 0; i < existingColumns.Count; i++)
					{
						if (string.Compare(existingColumns[i], columns[i], true) != 0)
							return false;
					}
				}
			}
			else
			{
				if (string.Compare(DependencyType, other.DependencyType, false) != 0)
					return false;

				if (string.Compare(DependencyProperty, other.DependencyProperty, false) != 0)
					return false;

				if (string.Compare(IndexGroup, other.IndexGroup, false) != 0)
					return false;
			}

			return true;
		}
	}
}
