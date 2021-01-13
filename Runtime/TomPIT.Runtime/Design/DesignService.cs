using TomPIT.Connectivity;

namespace TomPIT.Design
{
	internal class DesignService : TenantObject, IDesignService
	{
		private IVersionControl _versionControl = null;
		private IComponentModel _components = null;
		private IDesignSearch _search = null;

		public DesignService(ITenant tenant) : base(tenant)
		{
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

		public IVersionControl VersionControl
		{
			get
			{
				if (_versionControl == null)
					_versionControl = new VersionControl(Tenant);

				return _versionControl;
			}
		}
	}
}
