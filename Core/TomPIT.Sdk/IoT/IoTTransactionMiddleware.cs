using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ExceptionServices;
using Newtonsoft.Json;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.IoT
{
	public abstract class IoTTransactionMiddleware : MiddlewareOperation, IIoTTransactionMiddleware
	{
		[JsonIgnore]
		public string Name { get; set; }

		public void Invoke(IIoTDeviceMiddleware device)
		{
			try
			{

				Validate();
				AuthorizePolicies();
				OnAuthorize();
				OnInvoke(device);

				Invoked();
			}
			catch (ValidationException)
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

		protected virtual void OnAuthorize()
		{

		}

		protected virtual void OnInvoke(IIoTDeviceMiddleware device)
		{

		}

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return GetType().ShortName();

			return Name;
		}
	}
}
