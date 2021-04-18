namespace TomPIT.Cdn
{
	public interface IInboxAddress
	{
		string Name { get; }
		string Address { get; }
		bool IsInternational { get; }
	}
}
