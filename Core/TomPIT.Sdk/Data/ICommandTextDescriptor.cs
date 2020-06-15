using System.Collections.Generic;

namespace TomPIT.Data
{
	public interface ICommandTextDescriptor
	{
		CommandTextType Type { get; }
		CommandStatementType Statement { get; }
		string Procedure { get; }

		List<ICommandTextParameter> Parameters { get; }
		List<ICommandTextVariable> Variables { get; }

		public string CommandText { get; }
	}
}
