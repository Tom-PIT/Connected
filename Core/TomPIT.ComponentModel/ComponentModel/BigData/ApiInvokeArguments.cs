using Newtonsoft.Json.Linq;
using TomPIT.Services;

namespace TomPIT.ComponentModel.BigData
{
	public class ApiInvokeArguments : EventArguments
	{
		public ApiInvokeArguments(IExecutionContext sender, JArray items) : base(sender)
		{
			Items = items;
		}

		public JArray Items { get; }
	}
}
