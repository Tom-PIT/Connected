using System;
using TomPIT.ComponentModel;

namespace TomPIT.Middleware
{
	public class MicroServiceContext : MiddlewareContext, IMicroServiceContext
	{
		protected MicroServiceContext()
		{
			/*
			 * implementors will have their own initializing process 
			 * so we won't call initialize from default constructor
			 * the most common use is in the models
			 */
		}

		public MicroServiceContext(IMicroServiceContext context) : base(context)
		{
			MicroService = context.MicroService;
		}

		public MicroServiceContext(Guid microService) : this(microService, null)
		{
		}

		public MicroServiceContext(Guid microService, string endpoint) : base(endpoint)
		{
			if (Tenant != null)
				MicroService = Tenant.GetService<IMicroServiceService>().Select(microService);
		}

		public MicroServiceContext(IMicroService microService, string endpoint) : base(endpoint)
		{
			MicroService = microService;
		}

		public MicroServiceContext(IMicroService microService)
		{
			MicroService = microService;
		}

		public MicroServiceContext(IMicroService microService, IMiddlewareContext context) : base(context)
		{
			MicroService = microService;
		}

		public virtual IMicroService MicroService { get; protected set; }
	}
}
