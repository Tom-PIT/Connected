﻿using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Environment
{
	public class InstanceEndpointsModel : SynchronizedRepository<IInstanceEndpoint, Guid>
	{
		private ConcurrentDictionary<string, RoundRobin> _rr = new ConcurrentDictionary<string, RoundRobin>();

		public InstanceEndpointsModel(IMemoryCache container) : base(container, "instanceendpoint")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Environment.QueryInstanceEndpoints();

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

			var r = Shell.GetService<IDatabaseService>().Proxy.Environment.SelectInstanceEndpoint(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);

			if (r.Status == InstanceStatus.Enabled)
				Register(r.Features, r.Verbs, r.Token);
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

					var r = Shell.GetService<IDatabaseService>().Proxy.Environment.SelectInstanceEndpoint(token);

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
			var key = CreateRobinKey(features, verb);

			if (!_rr.ContainsKey(key))
				return null;

			var id = _rr[key].Next();

			if (id == Guid.Empty)
				return null;

			return Get(id);
		}

		public Guid Insert(InstanceFeatures features, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Environment.InsertInstanceEndpoint(token, features, name, url, reverseProxyUrl, status, verbs);

			Refresh(token);

			CachingNotifications.InstanceEndpointChanged(token);

			return token;
		}

		public void Update(Guid token, InstanceFeatures features, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var target = GetByToken(token);

			if (target == null)
				throw new SysException(SR.ErrInstanceEndpointNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Environment.UpdateInstanceEndpoint(target, features, name, url, reverseProxyUrl, status, verbs);

			Refresh(token);

			CachingNotifications.InstanceEndpointChanged(token);
		}

		public void Delete(Guid token)
		{
			var target = GetByToken(token);

			if (target == null)
				throw new SysException(SR.ErrInstanceEndpointNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Environment.DeleteInstanceEndpoint(target);

			Remove(token);
			CachingNotifications.InstanceEndpointRemoved(token);
		}
	}
}
