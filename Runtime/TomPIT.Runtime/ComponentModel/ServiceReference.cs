using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TomPIT.Annotations;
using TomPIT.Net;

namespace TomPIT.ComponentModel
{
	public class ServiceReference : Element, IServiceReference
	{
		private ISysContext _server = null;
		private string _ms = string.Empty;
		private string _name = string.Empty;

		[Items("TomPIT.Items.MicroServicesItems, TomPIT.Development")]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[InvalidateEnvironment(Ide.EnvironmentSection.Explorer | Ide.EnvironmentSection.Designer)]
		public string MicroService
		{
			get { return _ms; }
			set
			{
				if (string.IsNullOrWhiteSpace(value) || _server == null)
				{
					_ms = value;
					return;
				}

				CheckCyclicReference(value, new List<string>());

				_ms = value;
			}
		}

		public bool IsValid { get { return !string.IsNullOrWhiteSpace(_name); } }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(_name)
				? base.ToString()
				: _name;
		}

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			if (context.Context is ISysContext server)
			{
				_server = server;

				if (string.IsNullOrWhiteSpace(MicroService))
					_name = SR.ServiceReferenceNotSet;
				else
				{
					var ms = server.GetService<IMicroServiceService>().Select(MicroService);

					if (ms != null)
						_name = ms.Name;
					else
						_name = SR.InvalidServiceReference;
				}
			}
		}

		private void CheckCyclicReference(string microService, List<string> leads)
		{
			var me = this.Closest<IConfiguration>().MicroService(_server);
			var ms = _server.GetService<IMicroServiceService>().Select(me);

			if (ms == null)
				return;

			var refs = _server.GetService<IDiscoveryService>().References(microService);

			if (refs == null)
				return;

			foreach (var i in refs.MicroServices)
			{
				if (string.Compare(i.MicroService, ms.Name, true) == 0)
				{
					var rms = refs.MicroService(_server);
					var rs = _server.GetService<IMicroServiceService>().Select(i.MicroService);
					var title = i.MicroService.ToString();

					if (rs != null)
						title = rs.Name;

					throw new Exception(string.Format("{0} ({1})", SR.ErrCyclicReference, title));
				}

				if (leads.Contains(i.MicroService))
					continue;

				leads.Add(i.MicroService);

				CheckCyclicReference(i.MicroService, leads);
			}
		}
	}
}
