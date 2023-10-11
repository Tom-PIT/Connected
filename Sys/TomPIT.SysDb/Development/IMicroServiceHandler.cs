using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Environment;

namespace TomPIT.SysDb.Development
{
	public interface IMicroServiceHandler
	{
		List<IMicroService> Query();

		IMicroService SelectByUrl(string url);
		IMicroService Select(Guid token);
		IMicroService Select(string name);

		void Insert(Guid token, string name, string url, MicroServiceStages supportedStages, IResourceGroup resourceGroup, Guid template, string version);
		void Update(IMicroService microService, string name, string url, MicroServiceStages supportedStages, Guid template, IResourceGroup resourceGroup);
		void Delete(IMicroService microService);
	}
}
