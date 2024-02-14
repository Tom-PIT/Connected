using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Routing;
using TomPIT.Sys.Notifications;
using TomPIT.Sys.SourceFiles;

namespace TomPIT.Sys.Model.Components
{
	public class MicroServicesModel : SynchronizedRepository<IMicroService, Guid>
	{
		public MicroServicesModel(IMemoryCache container) : base(container, "microservice")
		{

		}

		public void Initialize(MicroServiceIndexEntry items)
		{

		}
		public IMicroService Select(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return null;

			return Get(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
		}

		public IMicroService SelectByUrl(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return null;

			return Get(f => string.Equals(f.Url, url, StringComparison.OrdinalIgnoreCase));
		}

		public IMicroService Select(Guid token)
		{
			return Get(token);
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
			var items = FileSystem.LoadMicroServices();

			foreach (var item in items)
				Set(item.Token, item, TimeSpan.Zero);
		}

		public void Insert(Guid token, string name, MicroServiceStages supportedStages, Guid resourceGroup, Guid template, string version, string commit)
		{
			_ = DataModel.ResourceGroups.Select(resourceGroup) ?? throw new SysException(SR.ErrResourceGroupNotFound);
			var url = Url(Guid.Empty, name);

			Set(token, new MicroServiceIndexEntry
			{
				Commit = commit,
				Name = name,
				ResourceGroup = resourceGroup,
				Template = template,
				SupportedStages = supportedStages,
				Token = token,
				Url = url,
				Version = version,
			}, TimeSpan.Zero);

			FileSystem.Serialize(All());
			CachingNotifications.MicroServiceChanged(token);
		}

		public void Update(Guid token, string name, MicroServiceStages supportedStages, Guid template, Guid resourceGroup, string version, string commit)
		{
			var microService = Select(token) ?? throw new SysException(SR.ErrMicroServiceNotFound);
			_ = DataModel.ResourceGroups.Select(resourceGroup) ?? throw new SysException(SR.ErrResourceGroupNotFound);
			var url = Url(token, name);

			if (template == Guid.Empty)
				template = microService.Template;

			Set(token, new MicroServiceIndexEntry
			{
				Commit = commit,
				Name = name,
				Template = template,
				SupportedStages = supportedStages,
				ResourceGroup = resourceGroup,
				Token = token,
				Url = url,
				Version = version
			}, TimeSpan.Zero);

			FileSystem.Serialize(All());
			CachingNotifications.MicroServiceChanged(token);
		}

		public void Delete(Guid token)
		{
			_ = Select(token) ?? throw new SysException(SR.ErrMicroServiceNotFound);

			Remove(token);

			FileSystem.Serialize(All());
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