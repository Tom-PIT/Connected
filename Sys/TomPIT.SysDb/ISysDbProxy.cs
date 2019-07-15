using TomPIT.SysDb.BigData;
using TomPIT.SysDb.Cdn;
using TomPIT.SysDb.Data;
using TomPIT.SysDb.Deployment;
using TomPIT.SysDb.Development;
using TomPIT.SysDb.Diagnostics;
using TomPIT.SysDb.Environment;
using TomPIT.SysDb.Events;
using TomPIT.SysDb.Globalization;
using TomPIT.SysDb.IoT;
using TomPIT.SysDb.Management;
using TomPIT.SysDb.Messaging;
using TomPIT.SysDb.Search;
using TomPIT.SysDb.Security;
using TomPIT.SysDb.Storage;
using TomPIT.SysDb.Workers;

namespace TomPIT.SysDb
{
	public interface ISysDbProxy
	{
		IDevelopmentHandler Development { get; }
		IManagementHandler Management { get; }
		ISecurityHandler Security { get; }
		IStorageHandler Storage { get; }
		IEnvironmentHandler Environment { get; }
		IGlobalizationHandler Globalization { get; }
		IDiagnosticHandler Diagnostics { get; }
		IWorkerHandler Workers { get; }
		IEventHandler Events { get; }
		IDataHandler Data { get; }
		IIoTHandler IoT { get; }
		IDeploymentHandler Deployment { get; }
		ICdnHandler Cdn { get; }
		IBigDataHandler BigData { get; }
		IMessagingHandler Messaging { get; }
		ISearchHandler Search { get; }

		void Initialize(string connectionString);
	}
}
