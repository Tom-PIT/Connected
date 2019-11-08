using System;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Globalization;
using TomPIT.Middleware;

namespace TomPIT.Management.Globalization
{
	internal class GlobalizationManagementService : TenantObject, IGlobalizationManagementService
	{
		public GlobalizationManagementService(ITenant tenant) : base(tenant)
		{
		}

		public void DeleteLanguage(Guid token)
		{
			var u = Tenant.CreateUrl("GlobalizationManagement", "DeleteLanguage");
			var e = new JObject
			{
				{"token", token }
			};

			Tenant.Post(u, e);

			if (Tenant.GetService<ILanguageService>() is ILanguageNotification n)
				n.NotifyRemoved(this, new LanguageEventArgs(token));
		}

		public Guid InsertLanguage(string name, int lcid, LanguageStatus status, string mappings)
		{
			var u = Tenant.CreateUrl("GlobalizationManagement", "InsertLanguage");
			var e = new JObject
			{
				{"name", name },
				{"lcid", lcid },
				{"status", status.ToString() },
				{"mappings", mappings }
			};

			var id = Tenant.Post<Guid>(u, e);

			if (Tenant.GetService<ILanguageService>() is ILanguageNotification n)
				n.NotifyChanged(this, new LanguageEventArgs(id));

			return id;
		}

		public void UpdateLanguage(Guid token, string name, int lcid, LanguageStatus status, string mappings)
		{
			var u = Tenant.CreateUrl("GlobalizationManagement", "UpdateLanguage");
			var e = new JObject
			{
				{"name", name },
				{"lcid", lcid },
				{"status", status.ToString() },
				{"mappings", mappings },
				{"token", token }
			};

			Tenant.Post(u, e);

			if (Tenant.GetService<ILanguageService>() is ILanguageNotification n)
				n.NotifyChanged(this, new LanguageEventArgs(token));
		}
	}
}
