using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.IoT;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareIoTService : MiddlewareObject, IMiddlewareIoTService
	{
		private string _server = null;

		public MiddlewareIoTService(IMiddlewareContext context) : base(context)
		{

		}

		public string Server
		{
			get
			{
				if (_server == null)
				{
					_server = Context.Services.Routing.GetServer(Environment.InstanceType.IoT, Environment.InstanceVerbs.All);

					if (_server == null)
						throw new RuntimeException(SR.ErrNoIoTServer);

					_server = $"{_server}/iot";
				}

				return _server;
			}
		}

		public List<IIoTFieldState> QueryState(string hub)
		{
			var descriptor = ComponentDescriptor.IoTHub(Context, hub);

			descriptor.Validate();

			return Context.Tenant.GetService<IIoTService>().SelectState(descriptor.Component.Token);
		}

		public IIoTFieldState SelectState(string hub, string field)
		{
			var state = QueryState(hub);

			if (state == null)
				return null;

			return state.FirstOrDefault(f => string.Compare(f.Field, field, true) == 0);
		}

		public T SelectValue<T>(string hub, string field)
		{
			var state = SelectState(hub, field);

			if (state == null)
				return default(T);

			if (Types.TryConvert<T>(state.Value, out T r))
				return r;

			return default(T);
		}

		public void Transaction(IoTMiddlewareTransactionArgs e)
		{
			var iotServer = Context.Services.Routing.GetServer(Environment.InstanceType.IoT, Environment.InstanceVerbs.All);
			var url = $"{iotServer}/transaction/{e.MicroService}/{e.Hub}/{e.Device}/{e.Transaction}";

			Context.Tenant.Post(url, e.Arguments);
		}
	}
}
