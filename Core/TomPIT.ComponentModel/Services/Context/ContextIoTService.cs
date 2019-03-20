using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.IoT;

namespace TomPIT.Services.Context
{
	internal class ContextIoTService : ContextClient, IContextIoTService
	{
		private string _server = null;

		public ContextIoTService(IExecutionContext context) : base(context)
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
			var tokens = hub.Split('/');
			var ms = Context.MicroService.Token;

			if (tokens.Length > 1)
			{
				Context.MicroService.ValidateMicroServiceReference(Context.Connection(), tokens[0]);

				var microService = Context.Connection().GetService<IMicroServiceService>().Select(tokens[0]);

				if (microService == null)
					throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({tokens[0]})");

				ms = microService.Token;
			}

			var component = Context.Connection().GetService<IComponentService>().SelectComponent(ms, "IoTHub", tokens[1]);

			if (component == null)
				throw new RuntimeException($"{SR.ErrIoTHubNotFound} ({tokens[1]})");

			return Context.Connection().GetService<IIoTService>().SelectState(component.Token);
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
	}
}
