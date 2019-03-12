using TomPIT.Connectivity;

namespace TomPIT.Services
{
	internal abstract class ServiceBase
	{
		public ServiceBase(ISysConnection connection)
		{
			Connection = connection;
		}

		protected ISysConnection Connection { get; }
	}
}
