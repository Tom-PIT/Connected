using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.SysDb.Events
{
	public interface IEventHandler
	{
		void Insert(IMicroService microService, string name, Guid identifier, DateTime created, string arguments, string callback);
		List<IEventDescriptor> Query();
		IEventDescriptor Select(Guid identifier);
		void Delete(IEventDescriptor d);
	}
}
