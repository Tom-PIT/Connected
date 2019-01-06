using TomPIT.Annotations;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface ITransactionResult
	{
		bool Success { get; }
		EnvironmentSection Invalidate { get; }
		object Component { get; }
	}
}
