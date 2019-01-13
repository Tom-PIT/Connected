using System;

namespace TomPIT.Security
{
	[Flags]
	public enum AuthenticationTokenClaim
	{
		None = 0,
		System = 1,
		Application = 2,
		Worker = 4,
		Cdn = 8,
		IoT = 16,
		BigData = 32,
		Search = 64,
		Rest = 128,
		All = 2147483647
	}

	public enum AuthenticationTokenStatus
	{
		Enabled = 1,
		Disabled = 2
	}

	public interface IAuthenticationToken
	{
		Guid Token { get; }
		string Key { get; }
		AuthenticationTokenClaim Claims { get; }
		AuthenticationTokenStatus Status { get; }
		DateTime ValidFrom { get; }
		DateTime ValidTo { get; }
		TimeSpan StartTime { get; }
		TimeSpan EndTime { get; }
		string IpRestrictions { get; }
		Guid ResourceGroup { get; }
		Guid User { get; }
		string Name { get; }
		string Description { get; }
	}
}
