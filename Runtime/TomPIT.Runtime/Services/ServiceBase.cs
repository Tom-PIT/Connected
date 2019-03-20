using TomPIT.Connectivity;

namespace TomPIT.Services
{
	public abstract class ServiceBase
	{
		public ServiceBase(ISysConnection connection)
		{
			Connection = connection;
		}

		protected ISysConnection Connection { get; }
	}
}
