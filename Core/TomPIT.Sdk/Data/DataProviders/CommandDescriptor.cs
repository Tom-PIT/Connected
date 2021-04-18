using System.Data;

namespace TomPIT.Data.DataProviders
{
	public class CommandDescriptor : ICommandDescriptor
	{
		public CommandType CommandType { get; set; }
		public string CommandText { get; set; }
	}
}
