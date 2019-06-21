using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel.BigData
{
	public class ApiCompleteArguments : DataModelContext
	{
		public ApiCompleteArguments(IExecutionContext sender, JArray items) : base(sender)
		{
			Items = items;
		}

		public JArray Items { get; }
	}
}
