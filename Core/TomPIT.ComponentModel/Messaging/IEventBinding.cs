namespace TomPIT.ComponentModel.Messaging
{
	public interface IEventBinding : IElement, IText
	{
		string Event { get; }
		string Name { get; }
	}
}
