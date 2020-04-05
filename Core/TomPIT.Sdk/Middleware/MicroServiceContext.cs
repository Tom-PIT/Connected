using System;
using Newtonsoft.Json;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Exceptions;

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

		public MicroServiceContext(Guid microService) : this(microService, string.Empty)
		{
		}

		public MicroServiceContext(Guid microService, IMiddlewareContext context) : base(context)
		{
			if (Tenant != null)
				MicroService = Tenant.GetService<IMicroServiceService>().Select(microService);
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

		public MicroServiceContext(IMicroService microService, IMiddlewareObject owner) : base(owner?.Context)
		{
			MicroService = microService;
		}
		[JsonIgnore]
		public virtual IMicroService MicroService { get; protected set; }

		public static IMicroServiceContext FromIdentifier(string identifier, ITenant tenant)
		{
			var tokens = identifier.Split('/');
			var ms = tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({tokens[0]})");

			return new MicroServiceContext(ms, tenant.Url);
		}
	}
}
