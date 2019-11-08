using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Middleware;

namespace TomPIT.Ide.ComponentModel
{
	internal class MicroServiceDevelopmentService : TenantObject, IMicroServiceDevelopmentService
	{
		public MicroServiceDevelopmentService(ITenant tenant) : base(tenant)
		{
		}

		public void DeleteString(Guid microService, Guid element, string property)
		{
			var u = Tenant.CreateUrl("MicroServiceDevelopment", "DeleteString");
			var args = new JObject
			{
				{ "microService",microService },
				{"element",element },
				{"property",property }
			};

			Tenant.Post(u, args);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyMicroServiceStringRemoved(this, new MicroServiceStringEventArgs(microService, Guid.Empty, element, property));
		}

		public void RestoreStrings(Guid microService, List<IPackageString> strings)
		{
			var u = Tenant.CreateUrl("MicroServiceDevelopment", "RestoreStrings");
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

			Tenant.Post(u, args);
		}

		public void UpdateString(Guid microService, Guid language, Guid element, string property, string value)
		{
			var u = Tenant.CreateUrl("MicroServiceDevelopment", "UpdateString");
			var args = new JObject
			{
				{ "microService", microService },
				{ "language",language },
				{"element", element },
				{"property", property },
				{"value", value },
			};

			Tenant.Post(u, args);

			if (Shell.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyMicroServiceStringChanged(this, new MicroServiceStringEventArgs(microService, language, element, property));
		}
	}
}
