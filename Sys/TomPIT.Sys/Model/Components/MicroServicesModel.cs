using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Routing;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Components
{
	public class MicroServicesModel : SynchronizedRepository<IMicroService, Guid>
	{
		public MicroServicesModel(IMemoryCache container) : base(container, "microservice")
		{

		}

		public IMicroService Select(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return null;

			var r = Get(f => string.Compare(f.Name, name, true) == 0);

			if (r != null)
				return r;

			var d = Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.Select(name);

			if (d == null)
				return null;

			Set(d.Token, d, TimeSpan.Zero);

			return d;
		}

		public IMicroService SelectByUrl(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return null;

			var r = Get(f => string.Compare(f.Url, url, true) == 0);

			if (r != null)
				return r;

			var d = Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.SelectByUrl(url);

			if (d == null)
				return null;

			Set(d.Token, d, TimeSpan.Zero);

			return d;
		}

		public IMicroService Select(Guid token)
		{
			return Get(token,
				 (f) =>
				 {
					 f.Duration = TimeSpan.Zero;

					 return Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.Select(token);
				 });
		}

		public ImmutableList<IMicroService> Query(Guid resourceGroup)
		{
			return Where(f => f.ResourceGroup == resourceGroup);
		}

		public ImmutableList<IMicroService> Query(List<Guid> resourceGroups)
		{
			return Where(f => resourceGroups.Any(t => t == f.ResourceGroup));
		}

		public ImmutableList<IMicroService> Query()
		{
			return All();
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public void Insert(Guid token, string name, MicroServiceStages supportedStages, Guid resourceGroup, Guid template, string version)
		{
			var g = DataModel.ResourceGroups.Select(resourceGroup);

			if (g == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var url = Url(Guid.Empty, name);

			Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.Insert(token, name, url, supportedStages, g, template, version);

			Refresh(token);

			CachingNotifications.MicroServiceChanged(token);
		}

		public void Update(Guid token, string name, MicroServiceStages supportedStages, Guid template, Guid resourceGroup)
		{
			var microService = Select(token);

			if (microService == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var g = DataModel.ResourceGroups.Select(resourceGroup);

			if (g == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var r = DataModel.ResourceGroups.Select(resourceGroup);

			if (r == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var url = microService.Url;

			//When running locally, the cached data is already modified here, so we need to generate the URL regardless
			url = Url(token, name);

			if (template == Guid.Empty)
				template = microService.Template;

			Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.Update(microService, name, url, supportedStages, template, r);

			Refresh(token);

			CachingNotifications.MicroServiceChanged(token);
		}

		public void Delete(Guid token)
		{
			var microService = Select(token);

			if (microService == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.Delete(microService);

			Remove(token);

			CachingNotifications.MicroServiceRemoved(token);
		}

		private string Url(Guid token, string name)
		{
			var ds = Query();
			var urls = new List<IUrlRecord>();

			foreach (var i in ds)
				urls.Add(new UrlRecord(i.Token.ToString(), i.Name));

			return UrlGenerator.GenerateUrl(token.ToString(), name, urls);
		}

	}
}