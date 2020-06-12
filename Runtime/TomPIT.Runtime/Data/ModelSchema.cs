using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TomPIT.Data
{
	internal class ModelSchema : IEquatable<ModelSchema>, IModelSchema
	{
		private List<IModelSchemaColumn> _columns = null;
		public List<IModelSchemaColumn> Columns
		{
			get
			{
				if (_columns == null)
					_columns = new List<IModelSchemaColumn>();

				return _columns;
			}
		}

		public string Schema { get; set; }
		public string Name { get; set; }

		public bool Equals([AllowNull] ModelSchema other)
		{
			if (other == null)
				return false;

			if (string.Compare(Name, other.Name, false) != 0)
				return false;

			if (string.Compare(Schema, other.Schema, false) != 0)
				return false;

			if (Columns.Count != other.Columns.Count)
				return false;

			for (var i = 0; i < Columns.Count; i++)
			{
				if (!Columns[i].Equals(other.Columns[i]))
					return false;
			}

			return true;
		}
	}
}
