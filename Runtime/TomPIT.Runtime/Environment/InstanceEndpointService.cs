using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Environment
{
	internal class InstanceEndpointService : SynchronizedClientRepository<IInstanceEndpoint, Guid>, IInstanceEndpointService, IInstanceEndpointNotification
	{
		private ConcurrentDictionary<string, RoundRobin> _rr = new ConcurrentDictionary<string, RoundRobin>();

		public InstanceEndpointService(ISysConnection connection) : base(connection, "instanceendpoint")
		{

		}

		protected override void OnInitializing()
		{
			var u = Connection.CreateUrl("InstanceEndpoint", "Query");
			var ds = Connection.Get<List<InstanceEndpoint>>(u);

			foreach (var i in ds)
			{
				Set(i.Token, i, TimeSpan.Zero);

				if (i.Status == InstanceStatus.Enabled)
					Register(i.Type, i.Verbs, i.Token);
			}
		}

		protected override void OnInvalidate(Guid id)
		{
			RemoveFromRobin(id);

			var d = Load(id);

			if (d == null)
			{
				Remove(id);
				return;
			}

			Set(id, d, TimeSpan.Zero);
		}

		public List<IInstanceEndpoint> Query()
		{
			return All();
		}

		public List<IInstanceEndpoint> Query(InstanceType type)
		{
			return Where(f => f.Type == type);
		}

		public IInstanceEndpoint Select(Guid endpoint)
		{
			return Get(endpoint,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var r = Load(endpoint);

					if (r != null && r.Status == InstanceStatus.Enabled)
						Register(r.Type, r.Verbs, r.Token);

					return r;
				});
		}

		public IInstanceEndpoint Select(InstanceType type)
		{
			return Next(type, InstanceVerbs.All);
		}

		private IInstanceEndpoint Load(Guid endpoint)
		{
			var u = Connection.CreateUrl("InstanceEndpoint", "Select")
				.AddParameter("endpoint", endpoint);

			return Connection.Get<InstanceEndpoint>(u);
		}

		public string Url(InstanceType type, InstanceVerbs verb)
		{
			var r = Next(type, verb);

			if (r == null)
				return null;

			return r.Url;
		}

		private void Register(InstanceType type, InstanceVerbs verbs, Guid endpoint)
		{
			if ((InstanceVerbs.Get & verbs) == InstanceVerbs.Get)
				Register(CreateRobinKey(type, InstanceVerbs.Get), endpoint);

			if ((InstanceVerbs.Post & verbs) == InstanceVerbs.Post)
				Register(CreateRobinKey(type, InstanceVerbs.Post), endpoint);

			if (verbs == InstanceVerbs.All)
				Register(CreateRobinKey(type, InstanceVerbs.All), endpoint);
		}

		private void Register(string key, Guid endpoint)
		{
			RoundRobin robin = null;

			if (_rr.ContainsKey(key))
				robin = _rr[key];
			else
			{
				robin = new RoundRobin();

				_rr.TryAdd(key, robin);
			}

			robin.Register(endpoint);
		}

		private void RemoveFromRobin(Guid endpoint)
		{
			foreach (var i in _rr.Keys)
				_rr[i].Remove(endpoint);
		}

		private string CreateRobinKey(InstanceType type, InstanceVerbs verbs)
		{
			return string.Format("{0}.{1}", type, verbs);
		}

		public IInstanceEndpoint Next(InstanceType type, InstanceVerbs verb)
		{
			var key = CreateRobinKey(type, verb);

			if (!_rr.ContainsKey(key))
				return null;

			var id = _rr[key].Next();

			if (id == Guid.Empty)
				return null;

			return Get(id);
		}

		public void NotifyChanged(object sender, InstanceEndpointEventArgs e)
		{
			Refresh(e.Endpoint);
		}

		public void NotifyRemoved(object sender, InstanceEndpointEventArgs e)
		{
			RemoveFromRobin(e.Endpoint);
			Remove(e.Endpoint);
		}
	}
}
