using Newtonsoft.Json;
using System;
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

        [Obsolete]
        public MicroServiceContext(Guid microService, string endpoint)
        {
            MicroService = Tenant.GetService<IMicroServiceService>().Select(microService);
        }

        public MicroServiceContext(Guid microService)
        {
            MicroService = Tenant.GetService<IMicroServiceService>().Select(microService);
        }

        public MicroServiceContext(IMicroService microService)
        {
            MicroService = microService;
        }

        [JsonIgnore]
        public virtual IMicroService MicroService { get; protected set; }

        public static IMicroServiceContext FromIdentifier(string identifier, ITenant tenant)
        {
            var tokens = identifier.Split('/');
            var ms = tenant.GetService<IMicroServiceService>().Select(tokens[0]) ?? throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({tokens[0]})");

            return new MicroServiceContext(ms);
        }
    }
}
