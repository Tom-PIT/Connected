using System;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class MicroServiceController : IMicroServiceController
	{
		public ImmutableList<IMicroService> Query()
		{
			return DataModel.MicroServices.Query();
		}

		public IMicroService Select(Guid microService)
		{
			return DataModel.MicroServices.Select(microService);
		}

		public IMicroService Select(string name)
		{
			return DataModel.MicroServices.Select(name);
		}

		public IMicroService SelectByUrl(string url)
		{
			return DataModel.MicroServices.SelectByUrl(url);
		}
	}
}
