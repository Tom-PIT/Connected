using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	public class InstanceEndpoints : SynchronizedRepository<IInstanceEndpoint, Guid>
	{
		private ConcurrentDictionary<string, RoundRobin> _rr = new ConcurrentDictionary<string, RoundRobin>();

		public InstanceEndpoints(IMemoryCache container) : base(container, "instanceendpoint")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Environment.QueryInstanceEndpoints();

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

			var r = Shell.GetService<IDatabaseService>().Proxy.Environment.SelectInstanceEndpoint(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);

			if (r.Status == InstanceStatus.Enabled)
				Register(r.Type, r.Verbs, r.Token);
		}

		public List<IInstanceEndpoint> Query()
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
						Register(r.Type, r.Verbs, r.Token);

					return r;
				});
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

		public Guid Insert(InstanceType type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Environment.InsertInstanceEndpoint(token, type, name, url, reverseProxyUrl, status, verbs);

			Refresh(token);

			CachingNotifications.InstanceEndpointChanged(token);

			return token;
		}

		public void Update(Guid token, InstanceType type, string name, string url, string reverseProxyUrl, InstanceStatus status, InstanceVerbs verbs)
		{
			var target = GetByToken(token);

			if (target == null)
				throw new SysException(SR.ErrInstanceEndpointNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Environment.UpdateInstanceEndpoint(target, type, name, url, reverseProxyUrl, status, verbs);

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
