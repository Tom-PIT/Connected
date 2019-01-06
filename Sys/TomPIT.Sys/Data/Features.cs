using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	internal class Features : SynchronizedRepository<IFeature, string>
	{
		public Features(IMemoryCache container) : base(container, "feature")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Development.Features.Query();

			foreach (var i in ds)
				Set(GenerateKey(i.MicroService, i.Token), i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			var s = DataModel.MicroServices.Select(tokens[0].AsGuid());

			if (s == null)
			{
				Remove(id);
				return;
			}

			var r = Shell.GetService<IDatabaseService>().Proxy.Development.Features.Select(s, tokens[1].AsGuid());

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IFeature Select(Guid microService, string name)
		{
			var r = Get(f => f.MicroService == microService && string.Compare(name, f.Name, true) == 0);

			if (r != null)
				return r;

			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			r = Shell.GetService<IDatabaseService>().Proxy.Development.Features.Select(s, name);

			if (r != null)
				Set(GenerateKey(microService, r.Token), r, TimeSpan.Zero);
			return r;
		}

		public IFeature Select(Guid microService, Guid token)
		{
			return Get(GenerateKey(microService, token),
				(f) =>
				{
					var s = DataModel.MicroServices.Select(microService);

					if (s == null)
						throw new SysException(SR.ErrMicroServiceNotFound);

					f.Duration = TimeSpan.Zero;

					return Shell.GetService<IDatabaseService>().Proxy.Development.Features.Select(s, token);
				});
		}

		public List<IFeature> Query(Guid microService)
		{
			return Where(f => f.MicroService == microService);
		}

		public Guid Insert(Guid microService, string name)
		{
			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var v = new Validator();

			v.Unique(null, name, nameof(IFeature.Name), Query(microService));

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			var token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Development.Features.Insert(s, name, token);

			var key = GenerateKey(microService, token);

			Refresh(key);
			NotificationHubs.FeatureChanged(microService, token);

			return token;
		}

		public void Update(Guid microService, Guid feature, string name)
		{
			var f = Select(microService, feature);

			if (f == null)
				throw new SysException(SR.ErrFeatureNotFound);

			var v = new Validator();

			v.Unique(f, name, nameof(IFeature.Name), Query(microService));

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			Shell.GetService<IDatabaseService>().Proxy.Development.Features.Update(f, name);

			var key = GenerateKey(microService, feature);

			Refresh(key);
			NotificationHubs.FeatureChanged(microService, feature);
		}

		public void Delete(Guid microService, Guid feature)
		{
			var f = Select(microService, feature);

			if (f == null)
				throw new SysException(SR.ErrFeatureNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Development.Features.Delete(f);

			var key = GenerateKey(microService, feature);

			Remove(key);
			NotificationHubs.FeatureRemoved(microService, feature);
		}
	}
}
