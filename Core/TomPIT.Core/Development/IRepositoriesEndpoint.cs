namespace TomPIT.Development
{
	public interface IRepositoriesEndpoint
	{
		string Name { get; }
		string Url { get; }
		string UserName { get; }
		byte[] Password { get; }
	}
}
