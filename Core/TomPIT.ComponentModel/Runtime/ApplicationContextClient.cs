using System;

namespace TomPIT.Runtime
{
	public abstract class ApplicationContextClient
	{
		public ApplicationContextClient(IApplicationContext context)
		{
			Context = context;
		}

		public IApplicationContext Context { get; }

		protected IExecutionContext CreateDescriptor(int eventId)
		{
			return CreateDescriptor(eventId, Context.Identity.Authority, Context.Identity.AuthorityId, null, Context.MicroService());
		}

		protected IExecutionContext CreateDescriptor(int eventId, string authority, string id)
		{
			return CreateDescriptor(eventId, authority, id, null, Context.MicroService());
		}

		protected IExecutionContext CreateDescriptor(int eventId, Guid microService)
		{
			return CreateDescriptor(eventId, null, null, null, microService);
		}

		protected IExecutionContext CreateDescriptor(int eventId, string authority, string id, string property)
		{
			return CreateDescriptor(eventId, authority, id, property, Context.MicroService());
		}

		protected IExecutionContext CreateDescriptor(int eventId, string authority, string id, string property, Guid microService)
		{
			return ExecutionContext.Create(eventId, authority, id, property, microService);
		}
	}
}
