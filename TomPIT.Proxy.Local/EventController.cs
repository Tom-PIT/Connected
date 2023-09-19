using System;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class EventController : IEventController
	{
		public Guid Trigger(Guid microService, string name, string callback, string arguments)
		{
			return DataModel.Events.Insert(microService, name, arguments, callback);
		}
	}
}
