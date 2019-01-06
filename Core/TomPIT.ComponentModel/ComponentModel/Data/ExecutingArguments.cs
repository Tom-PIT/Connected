using System.Data;
using TomPIT.ComponentModel.DataProviders;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel.Data
{
	public class ExecutingArguments : CancelEventArguments
	{
		internal ExecutingArguments(IApplicationContext sender, IDataCommandDescriptor command, DataTable schema) : base(sender)
		{
			Command = command;
			Schema = schema;
		}

		public IDataCommandDescriptor Command { get; }
		public DataTable Schema { get; set; }
	}
}
