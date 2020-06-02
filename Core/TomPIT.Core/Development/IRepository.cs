namespace TomPIT.Development
{
	public interface IRepository
	{
		string Name { get; }
		string Url { get; }
		string UserName { get; }
		byte[] Password { get; }
	}
}
