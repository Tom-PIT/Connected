using System.Data;

namespace TomPIT.Data.DataProviders
{
	public interface ICommandParameter
	{
		string Name { get; }
		DbType DataType { get; }
		object Value { get; set; }
		ParameterDirection Direction { get; }
	}
}
