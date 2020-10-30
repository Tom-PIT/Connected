using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Parsing
{
	internal class CommandTextVariable : ICommandTextVariable
	{
		public string Name { get; set; }
		public bool Bound { get; set; }
	}
}
