using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Net;

namespace TomPIT.ComponentModel
{
	internal class FeatureService : ContextCacheRepository<IFeature, string>, IFeatureService, IFeatureNotification
	{
		public event FeatureChangedHandler FeatureChanged;

		public FeatureService(ISysContext server) : base(server, "feature")
		{

		}

		public void NotifyChanged(object sender, FeatureEventArgs e)
		{
			Remove(f => f.MicroService == e.MicroService && f.Token == e.Feature);
			FeatureChanged?.Invoke(sender, e);
		}

		public List<IFeature> Query(Guid microService)
		{
			var u = Server.CreateUrl("Feature", "Query")
				.AddParameter("microService", microService);

			return Server.Connection.Get<List<Feature>>(u).ToList<IFeature>();
		}

		public IFeature Select(Guid microService, Guid feature)
		{
			var r = Get(f => f.MicroService == microService && f.Token == feature);

			if (r != null)
				return r;

			var u = Server.CreateUrl("Feature", "SelectByToken")
				.AddParameter("microService", microService)
				.AddParameter("feature", feature);

			r = Server.Connection.Get<Feature>(u);

			if (r != null)
				Set(GenerateRandomKey(), r);

			return r;
		}

		public IFeature Select(Guid microService, string name)
		{
			var r = Get(f => f.MicroService == microService && string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			var u = Server.CreateUrl("Feature", "Select")
				.AddParameter("microService", microService)
				.AddParameter("name", name);

			r = Server.Connection.Get<Feature>(u);

			if (r != null)
				Set(GenerateRandomKey(), r);

			return r;
		}
	}
}
