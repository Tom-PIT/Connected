using System.Collections.Generic;
using TomPIT.Data.DataProviders;

namespace TomPIT.Data
{
	public interface ICommandTextDescriptor
	{
		CommandTextType Type { get; }
		OperationType Statement { get; }
		string Name { get; }

		List<ICommandTextParameter> Parameters { get; }
		List<ICommandTextVariable> Variables { get; }

		public string CommandText { get; }
	}
}
