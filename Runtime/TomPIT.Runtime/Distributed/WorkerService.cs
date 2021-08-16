using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Distributed
{
    internal class WorkerService : MiddlewareObject, IWorkerService
    {
        public WorkerService(IMiddlewareContext context) : base(context)
        {

        }

        public void Run(Guid worker)
        {
            Context.Tenant.Post(Context.Tenant.CreateUrl("WorkerManagement", "Run"), new
            {
                worker
            });
		}
	}
}
