using TomPIT.Annotations;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public class TransactionResult : ITransactionResult
	{
		public TransactionResult(bool success)
		{
			Success = success;
		}

		public bool Success { get; private set; }
		public EnvironmentSection Invalidate { get; set; } = EnvironmentSection.None;
		public object Component { get; set; }
	}
}
