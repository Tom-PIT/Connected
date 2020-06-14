using System.Collections.Generic;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.IoT
{
	public abstract class IoTDeviceMiddleware : MiddlewareComponent, IIoTDeviceMiddleware
	{
		private List<IIoTTransactionMiddleware> _transactions = null;
		public void Invoke()
		{
			Validate();
			AuthorizePolicies();
			OnAuthorize();
			OnInvoke();
		}

		private void AuthorizePolicies()
		{
			Context.Tenant.GetService<IAuthorizationService>().AuthorizePolicies(Context, this);
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
			return GetType().ShortName();
		}
	}
}
