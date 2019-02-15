using System.Collections.Generic;
using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class Column : ITableColumn
	{
		private IReferentialConstraint _reference = null;
		private List<ITableConstraint> _constraints = null;

		public string Name { get; set; }
		public string DataType { get; set; }
		public bool Identity { get; set; }
		public bool IsNullable { get; set; }
		public string DefaultValue { get; set; }
		public int Ordinal { get; set; }
		public int CharacterMaximumLength { get; set; }
		public int CharacterOctetLength { get; set; }
		public int NumericPrecision { get; set; }
		public int NumericPrecisionRadix { get; set; }
		public int NumericScale { get; set; }
		public int DateTimePrecision { get; set; }
		public string CharacterSetName { get; set; }
		public IReferentialConstraint Reference
		{
			get
			{
				if (_reference == null)
					_reference = new ReferentialConstraint();

				return _reference;
			}
		}

		public List<ITableConstraint> Constraints
		{
			get
			{
				if (_constraints == null)
					_constraints = new List<ITableConstraint>();

				return _constraints;
			}
		}
	}
}
