using Newtonsoft.Json.Linq;
using TomPIT.Services;

namespace TomPIT.ComponentModel.BigData
{
	public class ApiCompleteArguments : EventArguments
	{
		public ApiCompleteArguments(IExecutionContext sender, JArray items) : base(sender)
		{
			Items = items;
		}

		public JArray Items { get; }
	}
}
