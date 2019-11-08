using System;

namespace TomPIT.Security.Authentication
{
	internal class AuthenticationToken : IAuthenticationToken
	{
		public Guid Token { get; set; }
		public string Key { get; set; }
		public AuthenticationTokenClaim Claims { get; set; }
		public AuthenticationTokenStatus Status { get; set; }
		public DateTime ValidFrom { get; set; }
		public DateTime ValidTo { get; set; }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public string IpRestrictions { get; set; }
		public Guid ResourceGroup { get; set; }
		public Guid User { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
