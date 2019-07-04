using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.IoT
{
	public interface IIoTTransactionHandler : IProcessHandler
	{
		void Invoke();
	}
}
