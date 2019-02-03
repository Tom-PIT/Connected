using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.Workers;

namespace TomPIT.Application.Workers
{
	[DefaultEvent(nameof(Invoke))]
	[DomDesigner("TomPIT.Designers.ScheduleDesigner, TomPIT.Management", Mode = Services.EnvironmentMode.Runtime)]
	public class HostedWorker : ComponentConfiguration, IHostedWorker
	{
		public const string ComponentCategory = "Worker";

		private IServerEvent _invoke = null;

		[EventArguments(typeof(WorkerInvokeArgs))]
		public IServerEvent Invoke
		{
			get
			{
				if (_invoke == null)
					_invoke = new ServerEvent { Parent = this };

				return _invoke;
			}
		}
	}
}
