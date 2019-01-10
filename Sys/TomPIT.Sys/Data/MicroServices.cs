using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Routing;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	internal class MicroServices : SynchronizedRepository<IMicroService, Guid>
	{
		public MicroServices(IMemoryCache container) : base(container, "microservice")
		{

		}

		public IMicroService Select(string name)
		{
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

		public List<IMicroService> Query(Guid resourceGroup)
		{
			return Where(f => f.ResourceGroup == resourceGroup);
		}

		public List<IMicroService> Query()
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

		public void Insert(Guid token, string name, MicroServiceStatus status, Guid resourceGroup, Guid template, string meta)
		{
			var g = DataModel.ResourceGroups.Select(resourceGroup);

			if (g == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var url = Url(Guid.Empty, name);

			Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.Insert(token, name, url, status, g, template, meta);

			Refresh(token);

			NotificationHubs.MicroServiceChanged(token);
		}

		public void Update(Guid token, string name, MicroServiceStatus status, Guid template, Guid resourceGroup)
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

			if (string.Compare(name, microService.Name, true) != 0)
				url = Url(token, name);

			if (template == Guid.Empty)
				template = microService.Template;

			Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.Update(microService, name, url, status, template, r);

			Refresh(token);

			NotificationHubs.MicroServiceChanged(token);
		}

		public void Delete(Guid token)
		{
			var microService = Select(token);

			if (microService == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.Delete(microService);

			Remove(token);

			NotificationHubs.MicroServiceRemoved(token);
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