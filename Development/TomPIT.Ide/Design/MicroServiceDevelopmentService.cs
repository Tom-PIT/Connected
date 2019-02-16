using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Deployment;

namespace TomPIT.Design
{
	internal class MicroServiceDevelopmentService : IMicroServiceDevelopmentService
	{
		public MicroServiceDevelopmentService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void DeleteString(Guid microService, Guid element, string property)
		{
			var u = Connection.CreateUrl("MicroServiceDevelopment", "DeleteString");
			var args = new JObject
			{
				{ "microService",microService },
				{"element",element },
				{"property",property }
			};

			Connection.Post(u, args);

			if (Connection.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyMicroServiceStringRemoved(this, new MicroServiceStringEventArgs(microService, Guid.Empty, element, property));
		}

		public void RestoreStrings(Guid microService, List<IPackageString> strings)
		{
			var u = Connection.CreateUrl("MicroServiceDevelopment", "RestoreStrings");
			var args = new JObject
			{
				{ "microService", microService }
			};

			var a = new JArray();

			args.Add("strings", a);

			foreach (var i in strings)
			{
				a.Add(new JObject
				{
					{"element", i.Element },
					{"lcid", i.Lcid },
					{"property", i.Property },
					{"value", i.Value }
				});
			}

			Connection.Post(u, args);
		}

		public void UpdateString(Guid microService, Guid language, Guid element, string property, string value)
		{
			var u = Connection.CreateUrl("MicroServiceDevelopment", "UpdateString");
			var args = new JObject
			{
				{ "microService", microService },
				{ "language",language },
				{"element", element },
				{"property", property },
				{"value", value },
			};

			Connection.Post(u, args);

			if (Shell.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyMicroServiceStringChanged(this, new MicroServiceStringEventArgs(microService, language, element, property));
		}
	}
}
