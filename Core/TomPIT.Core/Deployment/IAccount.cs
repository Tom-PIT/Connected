using System;

namespace TomPIT.Deployment
{
	public enum AccountStatus
	{
		Inactive = 1,
		Active = 2
	}

	public interface IAccount
	{
		string Company { get; }
		string Country { get; }
		string Website { get; }
		Guid Key { get; }
		AccountStatus Status { get; }
	}
}
