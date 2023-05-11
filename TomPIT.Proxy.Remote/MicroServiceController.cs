using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy.Remote
{
	internal class MicroServiceController : IMicroServiceController
	{
		private const string Controller = "MicroService";

		public ImmutableList<IMicroService> Query()
		{
			return Connection.Get<List<MicroService>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<IMicroService>();
		}

		public IMicroService Select(Guid microService)
		{
			var u = Connection.CreateUrl(Controller, "SelectByToken")
				.AddParameter("microService", microService);

			return Connection.Get<MicroService>(u);
		}

		public IMicroService Select(string name)
		{
			var u = Connection.CreateUrl(Controller, "Select")
				.AddParameter("name", name);

			return Connection.Get<MicroService>(u);
		}

		public IMicroService SelectByUrl(string url)
		{
			var u = Connection.CreateUrl(Controller, "SelectByUrl")
				.AddParameter("url", url);

			return Connection.Get<MicroService>(u);
		}

		public string SelectMeta(Guid microService)
		{
			var u = Connection.CreateUrl(Controller, "SelectMeta")
				.AddParameter("microService", microService);

			return Connection.Get<string>(u);
		}
	}
}
