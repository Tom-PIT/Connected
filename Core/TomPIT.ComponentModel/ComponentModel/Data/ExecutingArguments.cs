using System.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Data
{
	public class ExecutingArguments : CancelEventArguments
	{
		internal ExecutingArguments(IExecutionContext sender, IDataCommandDescriptor command, DataTable schema) : base(sender)
		{
			Command = command;
			Schema = schema;
		}

		public IDataCommandDescriptor Command { get; }
		public DataTable Schema { get; set; }
	}
}
