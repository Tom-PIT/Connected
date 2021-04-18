using System.Collections.Immutable;
using TomPIT.Data.DataProviders;

namespace TomPIT.Data
{
	public interface ICommandTextDescriptor
	{
		CommandTextType Type { get; }
		OperationType Statement { get; }
		string Name { get; }

		ImmutableArray<ICommandTextParameter> Parameters { get; }
		ImmutableArray<ICommandTextVariable> Variables { get; }

		string CommandText { get; }
		bool SupportsConcurrency { get; }
	}
}
