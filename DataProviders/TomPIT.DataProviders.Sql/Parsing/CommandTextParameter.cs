using System.Data;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Parsing
{
	internal class CommandTextParameter : ICommandTextParameter
	{
		public string Name { get; set; }
		public DbType Type { get; set; }
		public object Value { get; set; }
	}
}
