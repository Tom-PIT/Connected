using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using TomPIT.Connectivity;

namespace TomPIT.Design
{
	internal class DesignService : TenantObject, IDesignService
	{
		private IComponentModel _components = null;
		private IDesignSearch _search = null;
		private IDeployment _deployment = null;
		private ITextDiff _diff = null;
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

		public ITextDiff TextDiff
		{
			get
			{
				if (_diff == null)
					_diff = new TextDiff();

				return _diff;
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

		public IDesignSearch Search
		{
			get
			{
				if (_search == null)
					_search = new DesignSearch(Tenant);

				return _search;
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
			if (!Shell.Configuration.RootElement.TryGetProperty("designers", out JsonElement element))
				return;

			foreach (var item in element.EnumerateArray())
				Designers.Add(item.GetString());
		}
	}
}
