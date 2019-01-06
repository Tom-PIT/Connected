using System.Data;

namespace TomPIT.ComponentModel.DataProviders
{
	public interface ICommandDescriptor
	{
		CommandType CommandType { get; }
		string CommandText { get; }
	}
}