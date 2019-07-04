using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.IoT
{
	public abstract class IoTTransactionHandler: ProcessHandler, IIoTTransactionHandler
	{
		protected IoTTransactionHandler(IDataModelContext context) : base(context)
		{
		}

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected abstract void OnInvoke();
	}
}
