namespace TomPIT.ComponentModel.Distributed
{
	public interface IDistributedEvent : IText
	{
		string Name { get; }
	}
}
