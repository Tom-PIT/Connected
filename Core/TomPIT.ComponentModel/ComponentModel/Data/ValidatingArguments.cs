using TomPIT.Data.DataProviders;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Data
{
	public class ValidatingArguments : ValidatingEventArguments
	{
		public ValidatingArguments(IExecutionContext sender, IDataCommandDescriptor command) : base(sender)
		{
			Command = command;
		}

		public IDataCommandDescriptor Command { get; }
	}
}
