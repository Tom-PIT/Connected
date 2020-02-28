namespace TomPIT.ComponentModel.IoC
{
	public enum MasterDependencyKind
	{
		Server = 1,
		Client = 2
	}
	public interface IMasterDependency : IUIDependency
	{
		string Master { get; }
		MasterDependencyKind Kind { get; }
	}
}
