using System;
using TomPIT.Data.Sql;
using TomPIT.Environment;

namespace TomPIT.SysDb.Sql.Environment
{
	internal class InstanceEndpoint : PrimaryKeyRecord, IInstanceEndpoint
	{
		public string Url { get; set; }
		public InstanceStatus Status { get; set; }
		public string Name { get; set; }
		public InstanceFeatures Features { get; set; }
		public Guid Token { get; set; }
		public InstanceVerbs Verbs { get; set; }
		public string ReverseProxyUrl { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Url = GetString("url");
			Status = GetValue("status", InstanceStatus.Disabled);
			Name = GetString("name");
			Features = GetValue("type", InstanceFeatures.Unknown);
			Token = GetGuid("token");
			Verbs = GetValue("verbs", InstanceVerbs.Get);
			ReverseProxyUrl = GetString("reverse_proxy_url");
		}
	}
}
