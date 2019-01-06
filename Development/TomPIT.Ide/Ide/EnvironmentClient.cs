using TomPIT.Connectivity;

namespace TomPIT.Ide
{
	public class EnvironmentClient : IEnvironmentClient
	{
		public EnvironmentClient(IEnvironment environment)
		{
			Environment = environment;
		}

		public IEnvironment Environment { get; }

		protected ISysConnection Connection { get { return Environment.Context.Connection(); } }
	}
}
