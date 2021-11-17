using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Distributed;
using CI = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
    internal class MiddlewareWorkersService : MiddlewareObject, IMiddlewareWorkersService
    {
        public MiddlewareWorkersService(IMiddlewareContext context) : base(context)
        {
        }

        public void Run([CI(CI.WorkersProvider)] string worker)
        {
            var descriptor = ComponentDescriptor.HostedWorker(Context, worker);

            descriptor.Validate();
            descriptor.ValidateConfiguration();

            Context.Tenant.GetService<IWorkerService>().Run(descriptor.Configuration.Component);
        }
    }
}
