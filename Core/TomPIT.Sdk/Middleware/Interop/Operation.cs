using System;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
	public abstract class Operation : MiddlewareOperation, IOperation
	{
		public void Invoke()
		{
			Validate();

			try
			{
				OnInvoke();
			}
			catch (Exception ex)
			{
				throw new ScriptException(this, ex);
			}
		}

		protected virtual void OnInvoke()
		{

		}
	}
}
