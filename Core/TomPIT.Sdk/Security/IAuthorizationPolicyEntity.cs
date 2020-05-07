namespace TomPIT.Security
{
	public interface IAuthorizationPolicyEntity
	{
		string PrimaryKey { get; }
		string Name { get; }
	}
}
