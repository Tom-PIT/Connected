namespace TomPIT.ComponentModel.IoC
{
	public interface ISubscriptionDependency : IDependency
	{
		string Subscription { get; }
	}
}
