using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel.Features
{
	internal class FeatureService : ClientRepository<IFeature, string>, IFeatureService, IFeatureNotification
	{
		public event FeatureChangedHandler FeatureChanged;

		public FeatureService(ISysConnection connection) : base(connection, "feature")
		{

		}

		public void NotifyChanged(object sender, FeatureEventArgs e)
		{
			Remove(f => f.MicroService == e.MicroService && f.Token == e.Feature);
			FeatureChanged?.Invoke(sender, e);
		}

		public List<IFeature> Query(Guid microService)
		{
			var u = Connection.CreateUrl("Feature", "Query")
				.AddParameter("microService", microService);

			return Connection.Get<List<Feature>>(u).ToList<IFeature>();
		}

		public IFeature Select(Guid microService, Guid feature)
		{
			var r = Get(f => f.MicroService == microService && f.Token == feature);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("Feature", "SelectByToken")
				.AddParameter("microService", microService)
				.AddParameter("feature", feature);

			r = Connection.Get<Feature>(u);

			if (r != null)
				Set(GenerateRandomKey(), r);

			return r;
		}

		public IFeature Select(Guid microService, string name)
		{
			var r = Get(f => f.MicroService == microService && string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("Feature", "Select")
				.AddParameter("microService", microService)
				.AddParameter("name", name);

			r = Connection.Get<Feature>(u);

			if (r != null)
				Set(GenerateRandomKey(), r);

			return r;
		}
	}
}
