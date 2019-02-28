namespace TomPIT.Security
{
	public interface IUserData
	{
		string Topic { get; }
		string PrimaryKey { get; }
		string Value { get; }
	}
}
