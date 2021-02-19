using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Design
{
	internal class MicroServiceDesign : TenantObject, IMicroServiceDesign
	{
		public MicroServiceDesign(ITenant tenant) : base(tenant)
		{
		}

		public void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status)
		{
			Tenant.Post(CreateUrl("Insert"), new
			{
				microService = token,
				name,
				resourceGroup,
				template,
				status,
				meta = CreateMeta(token)
			});
		}

		public void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, UpdateStatus updateStatus, CommitStatus commitStatus)
		{
			Tenant.Post(CreateUrl("Update"), new
			{
				microService = token,
				name,
				resourceGroup,
				template,
				status,
				updateStatus,
				commitStatus
			});
		}

		/*
		 * Microservice meta is obsolete so this is a temporary fix.
		 */
		private string CreateMeta(Guid microService)
		{
			var meta = new JObject
				{
					 {"microService", microService },
					 {"created", DateTime.Today }
				};

			return Tenant.GetService<ICryptographyService>().Encrypt(JsonConvert.SerializeObject(meta));
		}

		private ServerUrl CreateUrl(string action)
		{
			return Tenant.CreateUrl("MicroServiceManagement", action);
		}
	}
}
