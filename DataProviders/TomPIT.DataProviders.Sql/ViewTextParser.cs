using System.Text.RegularExpressions;
using TomPIT.Data;
using TomPIT.DataProviders.Sql.Parsing;

namespace TomPIT.DataProviders.Sql
{
	internal class ViewTextParser : ICommandTextParser
	{
		public ICommandTextDescriptor Parse(string text)
		{
			var result = new CommandTextDescriptor();

			result.Parse(text);

			return result;
		}

		public static Regex ViewStatement { get; } = new Regex("^\\s*[^(--)]{0}(CREATE +VIEW|ALTER +VIEW)\\s+(?<ViewName>(\\w|_|\\[|\\]|\\.)*)");
	}
}
