using TomPIT.Connectivity;

namespace TomPIT.Design
{
	internal class DesignService : TenantObject, IDesignService
	{
		private IVersionControl _versionControl = null;
		private IComponentModel _components = null;
		private IDesignSearch _search = null;
		private IDeployment _deployment = null;
		private ITextDiff _diff = null;

		public DesignService(ITenant tenant) : base(tenant)
		{
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
