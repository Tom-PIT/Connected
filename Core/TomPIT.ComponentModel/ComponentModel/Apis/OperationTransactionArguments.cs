using TomPIT.Services;

namespace TomPIT.ComponentModel.Apis
{
	public class OperationTransactionArguments : OperationArguments
	{
		public OperationTransactionArguments(IExecutionContext sender, IApiOperation operation, IApiTransaction apiTransaction) : base(sender, operation, null)
		{
			ApiTransaction = apiTransaction;
		}

		public IApiTransaction ApiTransaction { get; }
	}
}
