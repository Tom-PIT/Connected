using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel.Events;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Apis
{
	public class OperationPrepareArguments : OperationArguments
	{
		public OperationPrepareArguments(IExecutionContext sender, IApiOperation operation, JObject arguments) : base(sender, operation, arguments)
		{
		}

		public IEventCallback RegisterAsync()
		{
			Async = true;

			return new EventCallback(this.MicroService(), Operation.Closest<IApi>().Component, Operation.Id);
		}

		public bool Async { get; private set; }
	}
}
