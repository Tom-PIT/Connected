using System.Text.RegularExpressions;
using TomPIT.Data;
using TomPIT.DataProviders.Sql.Parsing;

namespace TomPIT.DataProviders.Sql
{
	internal class ProcedureTextParser : ICommandTextParser
	{
		public ICommandTextDescriptor Parse(string text)
		{
			var result = new CommandTextDescriptor();

			result.Parse(text);

			return result;
		}

		public static Regex ProcedureStatement { get; } = new Regex("^\\s*[^(--)]{0}(CREATE +PROCEDURE|ALTER +PROCEDURE)\\s+(?<ProcName>(\\w|_|\\[|\\]|\\.)*)");
	}
}
