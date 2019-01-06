using TomPIT.Data.DataProviders;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Data
{
	public class TransactionExecutingArguments : CancelEventArguments
	{
		internal TransactionExecutingArguments(IExecutionContext sender, IDataCommandDescriptor command) : base(sender)
		{
			Command = command;
		}

		public IDataCommandDescriptor Command { get; }
	}
}
