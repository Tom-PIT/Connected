using System;

namespace TomPIT.Proxy.Remote
{
	internal class QueueController : IQueueController
	{
		private const string Controller = "Queue";
		public void Enqueue(Guid component, string name, string bufferKey, string arguments)
		{
			Enqueue(component, name, bufferKey, arguments, TimeSpan.FromDays(2), TimeSpan.Zero);
		}

		public void Enqueue(Guid component, string name, string bufferKey, string arguments, TimeSpan expire, TimeSpan nextVisible)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Enqueue"), new
			{
				component,
				name,
				expire,
				nextVisible,
				arguments,
				bufferKey
			});
		}
	}
}
