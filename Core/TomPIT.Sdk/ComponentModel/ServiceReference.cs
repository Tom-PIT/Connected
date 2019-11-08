using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TomPIT.Annotations.Design;
using TomPIT.Connectivity;
using TomPIT.Design.Validation;
using TomPIT.Reflection;

namespace TomPIT.ComponentModel
{
	[Glyph("fal fa-tilde")]
	public class ServiceReference : ConfigurationElement, IServiceReference
	{
		private ITenant _tenant = null;
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
				if (string.IsNullOrWhiteSpace(value) || _tenant == null)
				{
					_ms = value;
					return;
				}

				CheckCyclicReference(value, new List<string>());

				_ms = value;
			}
		}

		protected override void OnValidating(object sender, ElementValidationArgs e)
		{
			if (!_referenceValid)
				e.Error(SR.ErrValServiceNotSet);
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
			if (context.Context is ITenant tenant)
			{
				_tenant = tenant;

				if (string.IsNullOrWhiteSpace(MicroService))
					_name = SR.ServiceReferenceNotSet;
				else
				{
					var ms = tenant.GetService<IMicroServiceService>().Select(MicroService);

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
			var me = this.Configuration().MicroService();
			var ms = _tenant.GetService<IMicroServiceService>().Select(me);

			if (ms == null)
				return;

			var refs = _tenant.GetService<IDiscoveryService>().References(microService);

			if (refs == null)
				return;

			foreach (var i in refs.MicroServices)
			{
				if (string.Compare(i.MicroService, ms.Name, true) == 0)
				{
					var rms = refs.MicroService();
					var rs = _tenant.GetService<IMicroServiceService>().Select(i.MicroService);
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
