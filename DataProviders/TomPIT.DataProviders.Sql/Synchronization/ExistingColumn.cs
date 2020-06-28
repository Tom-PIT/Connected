using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomPIT.Annotations.Models;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class ExistingColumn : IModelSchemaColumn, IExistingModelSchemaColumn
	{
		public ExistingColumn(ExistingModel model)
		{
			Model = model;
		}

		private ExistingModel Model { get; }

		public string Name { get; set; }

		public DbType DataType { get; set; }

		public bool IsIdentity { get; set; }
		public bool IsVersion { get; set; }

		public bool IsUnique { get; set; }

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

		public List<string> QueryIndexColumns(string column)
		{
			foreach (var index in Model.Indexes)
			{
				if (index.Columns.Contains(column, StringComparer.OrdinalIgnoreCase))
					return index.Columns;
			}

			return new List<string>();
		}
	}
}
