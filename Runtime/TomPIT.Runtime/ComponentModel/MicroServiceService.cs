using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	internal class MicroServiceService : SynchronizedClientRepository<IMicroService, Guid>, IMicroServiceService, IMicroServiceNotification
	{
		public event MicroServiceChangedHandler MicroServiceChanged;
		public event MicroServiceChangedHandler MicroServiceInstalled;
		public event MicroServiceChangedHandler MicroServiceRemoved;

		public MicroServiceService(ITenant tenant) : base(tenant, "microservice")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Instance.SysProxy.MicroServices.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Instance.SysProxy.MicroServices.Select(id);

			if (r is null)
			{
				Remove(id);
				return;
			}

			Set(id, r);
		}

		public ImmutableList<IMicroService> Query()
		{
			return All();
		}

		public ImmutableList<IMicroService> Query(Guid user)
		{
			//TODO: perform micro service authorization
			return All();
		}

		public IMicroService Select(Guid microService)
		{
			var r = Get(microService);

			if (r is not null)
				return r;

			r = Instance.SysProxy.MicroServices.Select(microService);

			if (r is not null)
				Set(microService, r);

			return r;

		}

		public IMicroService Select(string name)
		{
			var r = Get(f => string.Compare(f.Name, name, true) == 0);

			if (r is not null)
				return r;

			r = Instance.SysProxy.MicroServices.Select(name);

			if (r is not null)
				Set(r.Token, r);

			return r;
		}

		public IMicroService SelectByUrl(string url)
		{
			var r = Get(f => string.Compare(f.Url, url, true) == 0);

			if (r is not null)
				return r;

			r = Instance.SysProxy.MicroServices.SelectByUrl(url);

			if (r is not null)
				Set(r.Token, r);

			return r;
		}

		public void NotifyChanged(object sender, MicroServiceEventArgs e)
		{
			Refresh(e.MicroService);
			MicroServiceChanged?.Invoke(sender, e);
		}

		public void NotifyMicroServiceInstalled(object sender, MicroServiceInstallEventArgs e)
		{
			Refresh(e.MicroService);

			MicroServiceInstalled?.Invoke(sender, e);
		}

		public void NotifyRemoved(object sender, MicroServiceEventArgs e)
		{
			Remove(e.MicroService);

			MicroServiceRemoved?.Invoke(sender, e);
		}
	}
}
