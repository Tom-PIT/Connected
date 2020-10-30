using System;

namespace TomPIT.Worker.Services
{
	internal interface IWorkerProxyService
	{
		void Ping(Guid microService, Guid popReceipt);
		void Error(Guid microService, Guid popReceipt);
		void Complete(Guid microService, Guid popReceipt);

		void AttachState(Guid worker, Guid state);
	}
}
