using System.Collections.Generic;

namespace TomPIT.Data.DataProviders.Deployment
{
	public interface ITableColumn
	{
		string Name { get; }
		string DataType { get; }
		bool Identity { get; }
		bool IsNullable { get; }
		string DefaultValue { get; }
		int Ordinal { get; }
		int CharacterMaximumLength { get; }
		int CharacterOctetLength { get; }
		int NumericPrecision { get; }
		int NumericPrecisionRadix { get; }
		int NumericScale { get; }
		int DateTimePrecision { get; }
		string CharacterSetName { get; }

		IReferentialConstraint Reference { get; }
		List<ITableConstraint> Constraints { get; }
	}
}
