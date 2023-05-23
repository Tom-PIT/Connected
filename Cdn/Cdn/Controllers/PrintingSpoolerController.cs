using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.Controllers;

namespace TomPIT.Cdn.Controllers
{
    [Authorize(AuthenticationSchemes = "TomPIT")]
    public class PrintingSpoolerController : ServerController
    {
        [HttpPost]
        public IPrintSpoolerJob SelectJob()
        {
            var body = FromBody();
            var id = body.Required<Guid>("id");
            var job = Instance.SysProxy.Management.Printing.SelectSpooler(id);

            if (job is null)
                return null;

            return job;
        }
    }
}
