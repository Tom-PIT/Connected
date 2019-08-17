using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Globalization;
using TomPIT.Services;

namespace TomPIT.Management.Globalization
{
	internal class GlobalizationManagementService :  ServiceBase, IGlobalizationManagementService
	{
		public GlobalizationManagementService(ISysConnection connection) : base(connection)
		{
		}

		public void DeleteLanguage(Guid token)
		{
			var u = Connection.CreateUrl("GlobalizationManagement", "DeleteLanguage");
			var e = new JObject
			{
				{"token", token }
			};

			Connection.Post(u, e);

			if (Connection.GetService<ILanguageService>() is ILanguageNotification n)
				n.NotifyRemoved(this, new LanguageEventArgs(token));
		}

		public Guid InsertLanguage(string name, int lcid, LanguageStatus status, string mappings)
		{
			var u = Connection.CreateUrl("GlobalizationManagement", "InsertLanguage");
			var e = new JObject
			{
				{"name", name },
				{"lcid", lcid },
				{"status", status.ToString() },
				{"mappings", mappings }
			};

			var id = Connection.Post<Guid>(u, e);

			if (Connection.GetService<ILanguageService>() is ILanguageNotification n)
				n.NotifyChanged(this, new LanguageEventArgs(id));

			return id;
		}

		public void UpdateLanguage(Guid token, string name, int lcid, LanguageStatus status, string mappings)
		{
			var u = Connection.CreateUrl("GlobalizationManagement", "UpdateLanguage");
			var e = new JObject
			{
				{"name", name },
				{"lcid", lcid },
				{"status", status.ToString() },
				{"mappings", mappings },
				{"token", token }
			};

			Connection.Post(u, e);

			if (Connection.GetService<ILanguageService>() is ILanguageNotification n)
				n.NotifyChanged(this, new LanguageEventArgs(token));
		}
	}
}
