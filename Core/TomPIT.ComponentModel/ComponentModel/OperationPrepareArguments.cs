using Newtonsoft.Json.Linq;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel
{
	public class OperationPrepareArguments : OperationArguments
	{
		public OperationPrepareArguments(IApplicationContext sender, IApiOperation operation, JObject arguments) : base(sender, operation, arguments)
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
