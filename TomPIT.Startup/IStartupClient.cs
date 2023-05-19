namespace TomPIT.Startup
{
	public interface IStartupClient
	{
		void Initialize(IStartupHost instance);
	}
}
