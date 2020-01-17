namespace TomPIT.ComponentModel.IoC
{
	public interface IApiDependency : IDependency
	{
		string Operation { get; }
	}
}
