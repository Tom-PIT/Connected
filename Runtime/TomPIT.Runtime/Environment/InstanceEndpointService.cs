using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Distributed;

namespace TomPIT.Environment
{
	internal class InstanceEndpointService : SynchronizedClientRepository<IInstanceEndpoint, Guid>, IInstanceEndpointService, IInstanceEndpointNotification
	{
		private ConcurrentDictionary<string, RoundRobin> _rr = new ConcurrentDictionary<string, RoundRobin>();

		public InstanceEndpointService(ITenant connection) : base(connection, "instanceendpoint")
		{

		}

		protected override void OnInitializing()
		{
			var ds = Instance.SysProxy.InstanceEndpoints.Query();

			foreach (var i in ds)
			{
				Set(i.Token, i, TimeSpan.Zero);

				if (i.Status == InstanceStatus.Enabled)
					Register(i.Features, i.Verbs, i.Token);
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

			if (d.Status == InstanceStatus.Enabled)
				Register(d.Features, d.Verbs, d.Token);

			Set(id, d, TimeSpan.Zero);
		}

		public ImmutableList<IInstanceEndpoint> Query()
		{
			return All();
		}

		public ImmutableList<IInstanceEndpoint> Query(InstanceFeatures features)
		{
			return Where(f => f.Features.HasFlag(features));
		}

		public IInstanceEndpoint Select(Guid endpoint)
		{
			return Get(endpoint,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var r = Load(endpoint);

					if (r != null && r.Status == InstanceStatus.Enabled)
						Register(r.Features, r.Verbs, r.Token);

					return r;
				});
		}

		public IInstanceEndpoint Select(InstanceFeatures features)
		{
			return Next(features, InstanceVerbs.All);
		}

		private IInstanceEndpoint Load(Guid endpoint)
		{
			return Instance.SysProxy.InstanceEndpoints.Select(endpoint);
		}

		public string Url(InstanceFeatures features, InstanceVerbs verb)
		{
			var r = Next(features, verb);

			if (r is null)
				return null;

			return r.Url;
		}

		private void Register(InstanceFeatures features, InstanceVerbs verbs, Guid endpoint)
		{
			foreach (var value in Enum.GetValues<InstanceFeatures>())
			{
				if (value == InstanceFeatures.Unknown || !features.HasFlag(value))
					continue;

				if ((InstanceVerbs.Get & verbs) == InstanceVerbs.Get)
					Register(CreateRobinKey(value, InstanceVerbs.Get), endpoint);

				if ((InstanceVerbs.Post & verbs) == InstanceVerbs.Post)
					Register(CreateRobinKey(value, InstanceVerbs.Post), endpoint);

				if (verbs == InstanceVerbs.All)
					Register(CreateRobinKey(value, InstanceVerbs.All), endpoint);
			}
		}

		private void Register(string key, Guid endpoint)
		{
			RoundRobin robin;

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

		private static string CreateRobinKey(InstanceFeatures features, InstanceVerbs verbs)
		{
			return $"{features}.{verbs}";
		}

		public IInstanceEndpoint Next(InstanceFeatures features, InstanceVerbs verb)
		{
			Initialize();

			var key = CreateRobinKey(features, verb);

			if (!_rr.ContainsKey(key))
			{
				if (verb == InstanceVerbs.All)
				{
					key = CreateRobinKey(features, InstanceVerbs.Get);

					if (!_rr.ContainsKey(key))
					{
						key = CreateRobinKey(features, InstanceVerbs.Post);

						if (!_rr.ContainsKey(key))
							return null;
					}
				}
				else
				{
					key = CreateRobinKey(features, InstanceVerbs.All);

					if (!_rr.ContainsKey(key))
						return null;
				}
			}

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
