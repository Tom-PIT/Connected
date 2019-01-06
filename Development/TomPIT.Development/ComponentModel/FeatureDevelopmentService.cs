using Newtonsoft.Json.Linq;
using System;
using TomPIT.ComponentModel.Features;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	internal class FeatureDevelopmentService : IFeatureDevelopmentService
	{
		public FeatureDevelopmentService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Delete(Guid microService, Guid feature)
		{
			var u = Connection.CreateUrl("FeatureDevelopment", "Delete");
			var args = new JObject
			{
				{"microService", microService },
				{ "feature", feature }
			};

			Connection.Post(u, args);

			if (Connection.GetService<IFeatureService>() is IFeatureNotification svc)
				svc.NotifyChanged(this, new FeatureEventArgs(microService, feature));
		}

		public Guid Insert(Guid microService, string name)
		{
			var u = Connection.CreateUrl("FeatureDevelopment", "Insert");
			var args = new JObject
			{
				{"microService", microService },
				{ "name", name }
			};

			var r = Connection.Post<Guid>(u, args);

			Connection.GetService<IFeatureService>().Select(microService, name);

			return r;
		}

		public void Update(Guid microService, Guid feature, string name)
		{
			var u = Connection.CreateUrl("FeatureDevelopment", "Update");
			var args = new JObject
			{
				{"microService", microService },
				{ "feature", feature },
				{ "name", name }
			};

			Connection.Post(u, args);

			if (Connection.GetService<IFeatureService>() is IFeatureNotification svc)
				svc.NotifyChanged(this, new FeatureEventArgs(microService, feature));
		}
	}
}
