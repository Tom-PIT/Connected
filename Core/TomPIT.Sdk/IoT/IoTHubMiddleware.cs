using System.Collections.Generic;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.IoT
{
	public abstract class IoTHubMiddleware<TSchema> : MiddlewareComponent, IIoTHubMiddleware<TSchema> where TSchema : class
	{
		private TSchema _schema = null;
		private List<IIoTTransactionMiddleware> _transactions = null;
		private List<IIoTDeviceMiddleware> _devices = null;

		public TSchema Schema
		{
			get
			{
				if (_schema == null)
					_schema = OnCreateSchema();

				return _schema;
			}
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

		public List<IIoTDeviceMiddleware> Devices
		{
			get
			{
				if (_devices == null)
				{
					_devices = OnCreateDevices();

					if (_devices != null)
					{
						foreach (var device in _devices)
							ReflectionExtensions.SetPropertyValue(device, nameof(device.Context), Context);
					}
				}

				return _devices;
			}
		}

		protected virtual TSchema OnCreateSchema()
		{
			return TypeExtensions.CreateInstance<TSchema>(typeof(TSchema));
		}

		protected virtual List<IIoTTransactionMiddleware> OnCreateTransactions()
		{
			return new List<IIoTTransactionMiddleware>();
		}

		protected virtual List<IIoTDeviceMiddleware> OnCreateDevices()
		{
			return new List<IIoTDeviceMiddleware>();
		}

		protected override void OnContextChanged()
		{
			if (Transactions != null)
			{
				foreach (var transaction in Transactions)
					ReflectionExtensions.SetPropertyValue(transaction, nameof(transaction.Context), Context);
			}

			if (Devices != null)
			{
				foreach (var device in Devices)
					ReflectionExtensions.SetPropertyValue(device, nameof(device.Context), Context);
			}

			base.OnContextChanged();
		}

		public void Authorize(IoTConnectionArgs e)
		{
			AuthorizePolicies(e.Method);
			OnAuthorize();
		}

		private void AuthorizePolicies(string method)
		{
			Context.Tenant.GetService<IAuthorizationService>().AuthorizePolicies(Context, this, method);
		}

		protected virtual void OnAuthorize()
		{

		}
	}
}
