using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ExceptionServices;
using Newtonsoft.Json;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.IoT
{
	public abstract class IoTDeviceMiddleware : MiddlewareOperation, IIoTDeviceMiddleware
	{
		private List<IIoTTransactionMiddleware> _transactions = null;
		public void Invoke()
		{
			try
			{
				Validate();
				AuthorizePolicies();
				OnAuthorize();
				OnInvoke();
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

		protected virtual void OnInvoke()
		{

		}

		public List<IIoTTransactionMiddleware> Transactions
		{
			get
			{
				if (_transactions == null)
				{
					_transactions = OnCreateTransactions();

					if (_transactions != null)
					{
						foreach (var transaction in _transactions)
							ReflectionExtensions.SetPropertyValue(transaction, nameof(transaction.Context), Context);
					}
				}

				return _transactions;
			}
		}

		[JsonIgnore]
		public string Name { get; set; }

		protected virtual List<IIoTTransactionMiddleware> OnCreateTransactions()
		{
			return new List<IIoTTransactionMiddleware>();
		}

		protected override void OnContextChanged()
		{
			if (Transactions != null)
			{
				foreach (var transaction in Transactions)
					ReflectionExtensions.SetPropertyValue(transaction, nameof(transaction.Context), Context);
			}

			base.OnContextChanged();
		}

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return GetType().ShortName();

			return Name;
		}
	}
}
