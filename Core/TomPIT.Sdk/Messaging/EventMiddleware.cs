using System;
using System.Collections.Generic;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.Messaging
{
	public abstract class EventMiddleware : MiddlewareOperation, IEventMiddleware
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

			try
			{
				Validate();
				OnInvoke();

				Invoked();
			}
			catch (System.ComponentModel.DataAnnotations.ValidationException)
			{
				Rollback();
				throw;
			}
			catch (Exception ex)
			{
				Rollback();
				throw new ScriptException(this, ex);
			}
		}

		protected abstract void OnInvoke();
	}
}
