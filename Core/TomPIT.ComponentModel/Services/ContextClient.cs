using System;

namespace TomPIT.Services
{
	public abstract class ContextClient
	{
		public ContextClient(IExecutionContext context)
		{
			Context = context;
		}

		public IExecutionContext Context { get; }

		protected IExecutionContextState CreateDescriptor(int eventId)
		{
			return CreateDescriptor(eventId, Context.Identity.Authority, Context.Identity.AuthorityId, null, Context.MicroService());
		}

		protected IExecutionContextState CreateDescriptor(int eventId, string authority, string id)
		{
			return CreateDescriptor(eventId, authority, id, null, Context.MicroService());
		}

		protected IExecutionContextState CreateDescriptor(int eventId, Guid microService)
		{
			return CreateDescriptor(eventId, null, null, null, microService);
		}

		protected IExecutionContextState CreateDescriptor(int eventId, string authority, string id, string property)
		{
			return CreateDescriptor(eventId, authority, id, property, Context.MicroService());
		}

		protected IExecutionContextState CreateDescriptor(int eventId, string authority, string id, string property, Guid microService)
		{
			return ExecutionContextState.Create(eventId, authority, id, property, microService);
		}
	}
}
