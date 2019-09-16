using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;

namespace TomPIT.Ide.Designers
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
		public JObject Data { get; set; }
	}
}
