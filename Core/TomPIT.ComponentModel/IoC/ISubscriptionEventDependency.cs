namespace TomPIT.ComponentModel.IoC
{
	public interface ISubscriptionEventDependency : IDependency
	{
		string Event { get; }
	}
}
