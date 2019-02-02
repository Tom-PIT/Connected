using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TomPIT.Analysis;
using TomPIT.Annotations;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	[Glyph("fal fa-tilde")]
	public class ServiceReference : ContextElement, IServiceReference
	{
		private ISysConnection _connection = null;
		private string _ms = string.Empty;
		private string _name = string.Empty;
		private bool _referenceValid = false;

		[Items("TomPIT.Design.Items.MicroServicesItems, TomPIT.Design")]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string MicroService
		{
			get { return _ms; }
			set
			{
				if (string.IsNullOrWhiteSpace(value) || _connection == null)
				{
					_ms = value;
					return;
				}

				CheckCyclicReference(value, new List<string>());

				_ms = value;
			}
		}

		protected override void OnValidate(object sender, ElementValidationArgs e)
		{
			if (!_referenceValid)
				e.Errors.Add(SR.ErrValServiceNotSet);
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(_name)
				? base.ToString()
				: _name;
		}

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			if (context.Context is ISysConnection server)
			{
				_connection = server;

				if (string.IsNullOrWhiteSpace(MicroService))
					_name = SR.ServiceReferenceNotSet;
				else
				{
					var ms = server.GetService<IMicroServiceService>().Select(MicroService);

					if (ms != null)
					{
						_name = ms.Name;
						_referenceValid = true;
					}
					else
						_name = SR.InvalidServiceReference;
				}
			}
		}

		private void CheckCyclicReference(string microService, List<string> leads)
		{
			var me = this.Closest<IConfiguration>().MicroService(_connection);
			var ms = _connection.GetService<IMicroServiceService>().Select(me);

			if (ms == null)
				return;

			var refs = _connection.GetService<IDiscoveryService>().References(microService);

			if (refs == null)
				return;

			foreach (var i in refs.MicroServices)
			{
				if (string.Compare(i.MicroService, ms.Name, true) == 0)
				{
					var rms = refs.MicroService(_connection);
					var rs = _connection.GetService<IMicroServiceService>().Select(i.MicroService);
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
