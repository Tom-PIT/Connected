using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

using TomPIT.Caching;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Environment
{
	public class InstanceEndpointsModel : CacheRepository<IInstanceEndpoint, Guid>
	{
		private ConcurrentDictionary<string, RoundRobin> _rr = new ConcurrentDictionary<string, RoundRobin>();

		public InstanceEndpointsModel(IMemoryCache container) : base(container, "instanceendpoint")
		{
			var ds = Shell.Configuration.GetSection("instanceEndpoints").Get<InstanceEndpointBindingModel[]>();

			foreach (var i in ds)
			{
				Set(i.Token, i, TimeSpan.Zero);

				if (i.Status == InstanceStatus.Enabled)
					Register(i.Features, i.Verbs, i.Token);
			}
		}

		public ImmutableList<IInstanceEndpoint> Query()
		{
			return All();
		}

		public IInstanceEndpoint GetByToken(Guid token)
		{
			return Get(token,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var r = Shell.Configuration.GetSection("instanceEndpoints").Get<InstanceEndpointBindingModel[]>().FirstOrDefault(e=> e.Token == token);

					if (r != null && r.Status == InstanceStatus.Enabled)
						Register(r.Features, r.Verbs, r.Token);

					return r;
				});
		}

		public string Url(InstanceFeatures features, InstanceVerbs verb)
		{
			var r = Next(features, verb);

			if (r == null)
				return null;

			return r.Url;
		}

		private void Register(InstanceFeatures features, InstanceVerbs verbs, Guid endpoint)
		{
			foreach (InstanceFeatures value in Enum.GetValues(typeof(InstanceFeatures)))
			{
				if (value != InstanceFeatures.Unknown && !features.HasFlag(value))
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

		private static string CreateRobinKey(InstanceFeatures features, InstanceVerbs verbs)
		{
			return $"{features}.{verbs}";
		}

		public IInstanceEndpoint Next(InstanceFeatures features, InstanceVerbs verb)
		{
			var key = CreateRobinKey(features, verb);

			if (!_rr.ContainsKey(key))
				return null;

			var id = _rr[key].Next();

			if (id == Guid.Empty)
				return null;

			return Get(id);
		}
				private class InstanceEndpointBindingModel : IInstanceEndpoint
		{
			public string? Url { get; set; }
			public InstanceStatus Status { get; set; } = InstanceStatus.Enabled;
			public string? Name { get; set; }
			public InstanceFeatures Features { get; set; }
			public Guid Token { get; set; } = Guid.NewGuid();
			public InstanceVerbs Verbs { get; set; } = InstanceVerbs.All;
			public string? ReverseProxyUrl { get; set; }

			public override string ToString()
			{
				return string.IsNullOrWhiteSpace(Name) ? base.ToString() : Name;
			}
		}
	}
}
