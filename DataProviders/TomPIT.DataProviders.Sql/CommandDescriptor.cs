using System.Data;
using TomPIT.Data.DataProviders;

namespace TomPIT.DataProviders.Sql
{
	internal class CommandDescriptor : ICommandDescriptor
	{
		public string CommandText
		{
			get; set;
		}

		public CommandType CommandType
		{
			get; set;
		}
	}
}
