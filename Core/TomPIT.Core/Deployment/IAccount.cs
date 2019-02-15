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
		int Country { get; }
		string Website { get; }
		string Key { get; }
		AccountStatus Status { get; }
	}
}
