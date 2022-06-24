using System;

namespace TomPIT.Middleware.Services
{
	public class IoTMiddlewareTransactionArgs : EventArgs
	{
		public string MicroService { get; set; }
		public string Hub { get; set; }
		public string Device { get; set; }
		public string Transaction { get; set; }
		public object Arguments { get; set; }
	}
}
