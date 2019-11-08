using System.Collections.Generic;
using Newtonsoft.Json;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment.Packages.Database
{
	public class TableColumn : ITableColumn
	{
		private IReferentialConstraint _reference = null;
		private List<ITableConstraint> _constraints = null;

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "dataType")]
		public string DataType { get; set; }
		[JsonProperty(PropertyName = "identity")]
		public bool Identity { get; set; }
		[JsonProperty(PropertyName = "isNullable")]
		public bool IsNullable { get; set; }
		[JsonProperty(PropertyName = "defaultValue")]
		public string DefaultValue { get; set; }
		[JsonProperty(PropertyName = "ordinal")]
		public int Ordinal { get; set; }
		[JsonProperty(PropertyName = "characterMaximumLength")]
		public int CharacterMaximumLength { get; set; }
		[JsonProperty(PropertyName = "characterOctetLength")]
		public int CharacterOctetLength { get; set; }
		[JsonProperty(PropertyName = "numericPrecision")]
		public int NumericPrecision { get; set; }
		[JsonProperty(PropertyName = "numericPrecisionRadix")]
		public int NumericPrecisionRadix { get; set; }
		[JsonProperty(PropertyName = "numericScale")]
		public int NumericScale { get; set; }
		[JsonProperty(PropertyName = "dateTimePrecision")]
		public int DateTimePrecision { get; set; }
		[JsonProperty(PropertyName = "characterSetName")]
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
