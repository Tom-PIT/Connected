using TomPIT.Connectivity;

namespace TomPIT.Services
{
	public abstract class ContextClient
	{
		public ContextClient(IExecutionContext context)
		{
			Context = context;
		}

		public IExecutionContext Context { get; }

		protected ISysConnection Connection => Context.Connection();
	}
}
