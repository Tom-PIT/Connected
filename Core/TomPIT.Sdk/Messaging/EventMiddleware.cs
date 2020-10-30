using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
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

				var se = new ScriptException(this, TomPITException.Unwrap(this, ex));

				ExceptionDispatchInfo.Capture(se).Throw();
			}
		}

		protected abstract void OnInvoke();
	}
}
