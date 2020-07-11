using System;
using System.Collections.Generic;
using TomPIT.Connectivity;
using TomPIT.Development;

namespace TomPIT.Design
{
	internal class DesignService : TenantObject, IDesignService
	{
		private IRepositories _repos = null;
		private IServiceBindings _bindings = null;
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

		public IRepositories Repositories
		{
			get
			{
				if (_repos == null)
					_repos = new Repositories(Tenant);

				return _repos;
			}
		}

		public IServiceBindings Bindings
		{
			get
			{
				if (_bindings == null)
					_bindings = new Bindings(Tenant);

				return _bindings;
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

		public void Pull(IServiceBinding binding)
		{
			throw new NotImplementedException();
		}

		public void Pull(IServiceBinding binding, List<Guid> components)
		{
			throw new NotImplementedException();
		}

		public void Push(IServiceBinding binding)
		{
			throw new NotImplementedException();
		}

		public void Push(IServiceBinding binding, List<Guid> components)
		{
			throw new NotImplementedException();
		}
	}
}
