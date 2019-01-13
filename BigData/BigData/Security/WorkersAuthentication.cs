using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;

namespace Amt.DataHub.Authentication
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class WorkersAuthentication : AuthorizeAttribute
	{
		public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
		{
			return true;
		}
		public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
		{
			try
			{
				if (ValidateWorker(request))
					return true;

				return false;
			}
			catch { return false; }
		}

		private bool ValidateWorker(IRequest request)
		{
			var id = request.Headers["WorkerId"];

			if (string.IsNullOrWhiteSpace(id))
				return false;

			var worker = DataHubModel.SelectWorker(id.AsGuid());

			return worker != null;
		}
	}
}
