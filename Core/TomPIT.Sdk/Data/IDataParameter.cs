using System.Data;

namespace TomPIT.Data
{
	public interface IDataParameter
	{
		string Name { get; set; }
		object Value { get; set; }
		ParameterDirection Direction { get; set; }

	}
}
