using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Distributed
{
    internal class WorkerService : TenantObject, IWorkerService
    {
        public WorkerService(ITenant tenant) : base(tenant)
        {

        }

        public void Run(Guid worker)
        {
            Tenant.Post(Tenant.CreateUrl("WorkerManagement", "Run"), new
            {
                worker
            });
		}
	}
}
