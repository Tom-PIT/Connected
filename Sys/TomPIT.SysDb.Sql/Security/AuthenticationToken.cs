using System;
using TomPIT.Data.Sql;
using TomPIT.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class AuthenticationToken : PrimaryKeyRecord, IAuthenticationToken
	{
		public Guid Token { get; set; }
		public string Key { get; set; }
		public AuthenticationTokenClaim Claims { get; set; } = AuthenticationTokenClaim.None;
		public AuthenticationTokenStatus Status { get; set; } = AuthenticationTokenStatus.Disabled;
		public DateTime ValidFrom { get; set; }
		public DateTime ValidTo { get; set; }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public string IpRestrictions { get; set; }
		public Guid ResourceGroup { get; set; }
		public Guid User { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Token = GetGuid("token");
			Key = GetString("key");
			Claims = GetValue("claims", AuthenticationTokenClaim.None);
			Status = GetValue("status", AuthenticationTokenStatus.Disabled);
			ValidFrom = GetDate("valid_from");
			ValidTo = GetDate("valid_to");
			StartTime = GetTimeSpan("start_time");
			EndTime = GetTimeSpan("end_time");
			IpRestrictions = GetString("ip_restrictions");
			ResourceGroup = GetGuid("resource_group_token");
			User = GetGuid("user_token");
			Name = GetString("name");
			Description = GetString("description");
		}
	}
}
