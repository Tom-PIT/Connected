using TomPIT.ComponentModel.DataProviders;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel.Data
{
	public class TransactionExecutingArguments : CancelEventArguments
	{
		internal TransactionExecutingArguments(IApplicationContext sender, IDataCommandDescriptor command) : base(sender)
		{
			Command = command;
		}

		public IDataCommandDescriptor Command { get; }
	}
}
