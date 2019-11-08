namespace TomPIT.Development.ComponentModel
{
	public interface IDependency
	{
		string Id { get; }
		string Title { get; }
		string Type { get; }
	}
}
