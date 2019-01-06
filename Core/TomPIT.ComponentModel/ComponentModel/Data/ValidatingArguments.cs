using TomPIT.ComponentModel.DataProviders;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel.Data
{
	public class ValidatingArguments : ValidatingEventArguments
	{
		public ValidatingArguments(IApplicationContext sender, IDataCommandDescriptor command) : base(sender)
		{
			Command = command;
		}

		public IDataCommandDescriptor Command { get; }
	}
}
