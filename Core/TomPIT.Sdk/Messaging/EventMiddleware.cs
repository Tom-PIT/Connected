using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Messaging
{
	public abstract class EventMiddleware : MiddlewareComponent, IEventMiddleware
	{
		private List<IOperationResponse> _responses = null;
		public string EventName { get; private set; }
		public List<IOperationResponse> Responses
		{
			get
			{
				if (_responses == null)
					_responses = new List<IOperationResponse>();

				return _responses;
			}
		}

		public void Invoke(string eventName)
		{
			EventName = eventName;

			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
