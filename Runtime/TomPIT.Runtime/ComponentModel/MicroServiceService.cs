using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.ComponentModel
{
	internal class MicroServiceService : SynchronizedClientRepository<IMicroService, Guid>, IMicroServiceService, IMicroServiceNotification
	{
		public event MicroServiceChangedHandler MicroServiceChanged;
		public event MicroServiceChangedHandler MicroServiceInstalled;
		private MicroServiceMetaCache _meta = null;

		public MicroServiceService(ISysConnection connection) : base(connection, "microservice")
		{
			_meta = new MicroServiceMetaCache(Connection);
		}

		protected override void OnInitializing()
		{
			var u = Connection.CreateUrl("MicroService", "Query");
			var ds = Connection.Get<List<MicroService>>(u).ToList<IMicroService>();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Connection.CreateUrl("MicroService", "SelectByToken")
				.AddParameter("microService", id);

			var r = Connection.Get<MicroService>(u);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r);
		}

		public List<IMicroService> Query()
		{
			return All();
		}

		public List<IMicroService> Query(Guid user)
		{
			//TODO: perform micro service authorization
			return All();
		}

		public IMicroService Select(Guid microService)
		{
			var r = Get(microService);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("MicroService", "SelectByToken")
				.AddParameter("microService", microService);

			r = Connection.Get<MicroService>(u);

			if (r != null)
				Set(microService, r);

			return r;

		}

		public IMicroService Select(string name)
		{
			var r = Get(f => string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("MicroService", "Select")
				.AddParameter("name", name);

			r = Connection.Get<MicroService>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public IMicroService SelectByUrl(string url)
		{
			var r = Get(f => string.Compare(f.Url, url, true) == 0);

			if (r != null)
				return r;

			var u = Connection.CreateUrl("MicroService", "SelectByUrl")
				.AddParameter("url", url);

			r = Connection.Get<MicroService>(u);

			if (r != null)
				Set(r.Token, r);

			return r;
		}

		public string SelectString(Guid microService, Guid language, Guid element, string property)
		{
			var key = GenerateKey(microService, language, "microservicestring");

			if (!Container.Exists(key))
			{
				var u = Connection.CreateUrl("MicroService", "QueryStrings")
					.AddParameter("microService", microService)
					.AddParameter("language", language);

				var r = Connection.Get<List<MicroServiceString>>(u).ToList<IMicroServiceString>();

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

		public void NotifyMicroServiceInstalled(object sender, MicroServiceEventArgs e)
		{
			MicroServiceInstalled?.Invoke(sender, e);
		}

		public void NotifyRemoved(object sender, MicroServiceEventArgs e)
		{
			Remove(e.MicroService);
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
