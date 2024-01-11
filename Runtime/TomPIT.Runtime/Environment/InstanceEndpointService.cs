using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Distributed;

namespace TomPIT.Environment
{
	internal class InstanceEndpointService : CacheRepository<IInstanceEndpoint, Guid>, IInstanceEndpointService
	{
		private ConcurrentDictionary<string, RoundRobin> _rr = new ConcurrentDictionary<string, RoundRobin>();

		public InstanceEndpointService(ITenant connection) : base(connection.Cache, "instanceendpoint")
		{
			var endpoints = Shell.Configuration.GetSection("instanceEndpoints").Get<InstanceEndpointBindingModel[]>() ?? Array.Empty<InstanceEndpointBindingModel>();
			foreach (var i in endpoints)
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

		private IInstanceEndpoint? Load(Guid endpoint)
		{
			var endpointConfiguration = Shell.Configuration.GetSection("instanceEndpoints").Get<InstanceEndpointBindingModel[]>() ?? Array.Empty<InstanceEndpointBindingModel>();
			return endpointConfiguration.FirstOrDefault(e => e.Token == endpoint);
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

			if (_rr.TryGetValue(key, out RoundRobin? value))
				robin = value;
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
