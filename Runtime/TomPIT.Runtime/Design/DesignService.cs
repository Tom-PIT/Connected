using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using TomPIT.Connectivity;

namespace TomPIT.Design
{
	internal class DesignService : TenantObject, IDesignService
	{
		private IComponentModel _components = null;
		private IDeployment _deployment = null;
		private IMicroServiceDesign _microServices = null;

		public DesignService(ITenant tenant) : base(tenant)
		{
			Designers = new();

			InitializeConfiguration();
		}

		private List<string> Designers { get; }

		public ImmutableList<string> QueryDesigners()
		{
			return Designers.ToImmutableList();
		}

		public IMicroServiceDesign MicroServices
		{
			get
			{
				if (_microServices == null)
					_microServices = new MicroServiceDesign(Tenant);

				return _microServices;
			}
		}

		public IDeployment Deployment
		{
			get
			{
				if (_deployment == null)
					_deployment = new Deployment(Tenant);

				return _deployment;
			}
		}

		public IComponentModel Components
		{
			get
			{
				if (_components == null)
					_components = new Components(Tenant);

				return _components;
			}
		}

		public void Initialize()
		{
			((Deployment)Deployment).Initialize();
		}

		private void InitializeConfiguration()
		{
			var designers = Shell.Configuration.GetSection("designers").Get<string[]>();

			foreach (var item in designers)
				Designers.Add(item);
		}
	}
}
