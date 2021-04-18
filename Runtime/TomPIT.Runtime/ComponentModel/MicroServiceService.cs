using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.ComponentModel
{
	internal class MicroServiceService : SynchronizedClientRepository<IMicroService, Guid>, IMicroServiceService, IMicroServiceNotification
	{
		public event MicroServiceChangedHandler MicroServiceChanged;
		public event MicroServiceChangedHandler MicroServiceInstalled;
		public event MicroServiceChangedHandler MicroServiceRemoved;

		private MicroServiceMetaCache _meta = null;

		public MicroServiceService(ITenant tenant) : base(tenant, "microservice")
		{
			_meta = new MicroServiceMetaCache(Tenant);
		}

		protected override void OnInitializing()
		{
			var u = Tenant.CreateUrl("MicroService", "Query");
			var ds = Tenant.Get<List<MicroService>>(u).ToList<IMicroService>();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Tenant.CreateUrl("MicroService", "SelectByToken")
				.AddParameter("microService", id);

			var r = Tenant.Get<MicroService>(u);

			if (r == null)
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

			if (r != null)
				return r;

			var u = Tenant.CreateUrl("MicroService", "SelectByToken")
				.AddParameter("microService", microService);

			r = Tenant.Get<MicroService>(u);

			if (r != null)
				Set(microService, r);

			return r;

		}

		public IMicroService Select(string name)
		{
			var r = Get(f => string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			var u = Tenant.CreateUrl("MicroService", "Select")
				.AddParameter("name", name);

			r = Tenant.Get<MicroService>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IMicroService SelectByUrl(string url)
		{
			var r = Get(f => string.Compare(f.Url, url, true) == 0);

			if (r != null)
				return r;

			var u = Tenant.CreateUrl("MicroService", "SelectByUrl")
				.AddParameter("url", url);

			r = Tenant.Get<MicroService>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public string SelectString(Guid microService, Guid language, Guid element, string property)
		{
			var key = GenerateKey(microService, language, "microservicestring");

			if (!Container.Exists(key))
			{
				var u = Tenant.CreateUrl("MicroService", "QueryStrings")
					.AddParameter("microService", microService)
					.AddParameter("language", language);

				var r = Tenant.Get<List<MicroServiceString>>(u).ToList<IMicroServiceString>();

				foreach (var i in r)
					Container.Set(key, GenerateKey(microService, language, element, property), i, TimeSpan.Zero);
			}

			return Container.Get<IMicroServiceString>(key, GenerateKey(microService, language, element, property))?.Value;
		}

		public void NotifyChanged(object sender, MicroServiceEventArgs e)
		{
			Refresh(e.MicroService);
			MicroServiceChanged?.Invoke(sender, e);
		}

		public void NotifyMicroServiceStringChanged(object sender, MicroServiceStringEventArgs e)
		{
			var key = GenerateKey(e.MicroService, e.Language, "microservicestring");

			Container.Clear(key);
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

		public void NotifyMicroServiceStringRemoved(object sender, MicroServiceStringEventArgs e)
		{
		}

		public string SelectMeta(Guid microService)
		{
			return Meta.Select(microService);
		}

		public MicroServiceMetaCache Meta
		{
			get { return _meta; }
		}
	}
}
