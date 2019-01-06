using TomPIT.Runtime;

namespace TomPIT.ComponentModel
{
	public class OperationTransactionArguments : OperationArguments
	{
		public OperationTransactionArguments(IApplicationContext sender, IApiOperation operation, IApiTransaction apiTransaction) : base(sender, operation, null)
		{
			ApiTransaction = apiTransaction;
		}

		public IApiTransaction ApiTransaction { get; }
	}
}
