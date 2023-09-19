using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Api.Storage;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Controllers.Management
{
    public class StorageManagementController : SysController
    {
        [HttpGet]
        public List<IBlob> QueryOrphanedDrafts()
        {
            return Shell.GetService<IDatabaseService>().Proxy.Storage.QueryOrphaned(DateTime.UtcNow.AddDays(-1));
        }

        [HttpGet]
        public List<IClientStorageProvider> QueryStorageProviders()
        {
            return Shell.GetService<IStorageProviderService>().Query().ToList<IClientStorageProvider>();
        }
    }
}
