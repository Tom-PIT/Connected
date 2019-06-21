using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel.BigData
{
	public class ApiInvokeArguments : DataModelContext
	{
		public ApiInvokeArguments(IExecutionContext sender, JArray items) : base(sender)
		{
			Items = items;
		}

		public JArray Items { get; }
	}
}
