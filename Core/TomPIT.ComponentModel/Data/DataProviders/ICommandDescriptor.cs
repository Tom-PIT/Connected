using System.Data;

namespace TomPIT.Data.DataProviders
{
	public interface ICommandDescriptor
	{
		CommandType CommandType { get; }
		string CommandText { get; }
	}
}