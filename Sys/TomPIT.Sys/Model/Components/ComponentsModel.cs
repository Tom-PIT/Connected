using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Api.ComponentModel;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Sys.Notifications;
using TomPIT.Sys.SourceFiles;

namespace TomPIT.Sys.Model.Components
{
	public class ComponentsModel : SynchronizedRepository<IComponent, Guid>
	{
		public ComponentsModel(IMemoryCache container) : base(container, "component")
		{
		}

		protected override void OnInitializing()
		{
			var ds = FileSystem.LoadComponents();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		//public void RefreshComponent(Guid token)
		//{
		//	Refresh(token);

		//	DataModel.Blobs.RefreshBlob(token);
		//}

		public void NotifyChanged(IComponent component)
		{
			CachingNotifications.ComponentChanged(component.MicroService, component.Folder, component.Token, component.NameSpace, component.Category, component.Name);
		}

		public IComponent Select(Guid token)
		{
			return Get(token);
		}

		public IComponent SelectByNameSpace(Guid microService, string nameSpace, string name)
		{
			var r = Where(f => f.MicroService == microService && string.Compare(f.Name, name, true) == 0
				 && string.Compare(f.NameSpace, nameSpace, true) == 0);

			if (r is not null && r.Any())
			{
				if (r.Count > 1)
					throw new SysException(string.Format("{0} ({1}.{2})", SR.ErrDuplicateComponentFound, nameSpace, name));

				return r[0];
			}

			return null;
		}

		public IComponent Select(string category, string name)
		{
			var r = Where(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)
				 && string.Equals(f.Category, category, StringComparison.OrdinalIgnoreCase));

			if (r is not null && r.Any())
			{
				if (r.Count > 1)
					throw new SysException(string.Format("{0} ({1}.{2})", SR.ErrDuplicateComponentFound, category, name));

				return r[0];
			}

			return null;
		}

		public IComponent Select(Guid microService, string category, string name)
		{
			return Get(f => f.MicroService == microService
				 && string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)
				 && string.Equals(f.Category, category, StringComparison.OrdinalIgnoreCase));
		}

		public ImmutableList<IComponent> QueryCategories(Guid microService, string categories)
		{
			var cats = categories.Split(',');
			var r = new List<IComponent>();

			foreach (var j in cats)
			{
				if (string.IsNullOrWhiteSpace(j))
					continue;

				var ds = Query(microService, j.Trim());

				if (ds.Count > 0)
					r.AddRange(ds);
			}

			return r.ToImmutableList();
		}

		public ImmutableList<IComponent> QueryByCategories(string categories)
		{
			var cats = categories.Split(',');
			var r = new List<IComponent>();

			foreach (var j in cats)
			{
				if (string.IsNullOrWhiteSpace(j))
					continue;

				var ds = Where(f => string.Equals(f.Category, j.Trim(), StringComparison.OrdinalIgnoreCase));

				if (ds.Any())
					r.AddRange(ds);
			}

			return r.ToImmutableList();
		}

		public ImmutableList<IComponent> Query(string resourceGroups, string categories)
		{
			var tokens = string.IsNullOrWhiteSpace(resourceGroups) ? Array.Empty<string>() : resourceGroups.Split(',', StringSplitOptions.RemoveEmptyEntries);
			var cats = string.IsNullOrWhiteSpace(categories) ? Array.Empty<string>() : categories.Split(',', StringSplitOptions.RemoveEmptyEntries);

			var r = new List<IComponent>();
			var microServices = new List<IMicroService>();

			if (tokens.Length == 0)
			{
				var ms = DataModel.MicroServices.Query();

				if (ms != null && ms.Count > 0)
					microServices.AddRange(ms);
			}
			else
			{
				foreach (var i in tokens)
				{
					var rs = DataModel.ResourceGroups.Select(i);

					if (rs == null)
						throw new SysException(string.Format("{0} ({1})", SR.ErrResourceGroupNotFound, i));

					var sols = DataModel.MicroServices.Query(rs.Token);

					if (sols.Count > 0)
						microServices.AddRange(sols);
				}
			}

			foreach (var i in microServices)
			{
				if (cats.Length == 0)
				{
					var ds = Query(i.Token);

					if (ds.Count > 0)
						r.AddRange(ds);
				}
				else
				{
					foreach (var j in cats)
					{
						if (string.IsNullOrWhiteSpace(j))
							continue;

						var ds = Query(i.Token, j.Trim());

						if (ds.Count > 0)
							r.AddRange(ds);
					}
				}
			}

			return r.ToImmutableList();
		}

		public ImmutableList<IComponent> Query()
		{
			return All();
		}

		public ImmutableList<IComponent> Query(Guid[] microService)
		{
			return Where(f => microService.Any(g => g == f.MicroService));
		}

		public ImmutableList<IComponent> Query(Guid microService)
		{
			return Where(f => f.MicroService == microService);
		}

		public ImmutableList<IComponent> Query(Guid microService, Guid folder)
		{
			return Where(f => f.MicroService == microService && f.Folder == folder);
		}

		public ImmutableList<IComponent> QueryByNameSpace(Guid microService, string nameSpace)
		{
			return Where(f => f.MicroService == microService && string.Equals(f.NameSpace, nameSpace, StringComparison.OrdinalIgnoreCase));
		}

		public ImmutableList<IComponent> Query(Guid microService, string category)
		{
			return Where(f => f.MicroService == microService && string.Equals(f.Category, category, StringComparison.OrdinalIgnoreCase));
		}

		public void Insert(Guid component, Guid microService, Guid folder, string category, string nameSpace, string name, string type)
		{
			var s = DataModel.MicroServices.Select(microService) ?? throw new SysException(SR.ErrMicroServiceNotFound);
			s.DemandDevelopmentStage();

			IFolder f = folder == Guid.Empty ? null : DataModel.Folders.Select(folder);

			if (folder != Guid.Empty && f is null)
				throw new SysException(SR.ErrFolderNotFound);

			var v = new Validator();

			v.Unique(null, name, nameof(IComponent.Name), QueryByNameSpace(microService, nameSpace));

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			Set(component, new ComponentIndexEntry
			{
				Category = category,
				Name = name,
				Type = type,
				Folder = folder,
				MicroService = microService,
				Modified = DateTime.UtcNow,
				NameSpace = nameSpace,
				Token = component
			}, TimeSpan.Zero);

			FileSystem.Serialize(All());
			CachingNotifications.ComponentAdded(microService, folder, component, nameSpace, category, name);
		}

		public void UpdateModified(Guid microService, string category, string name)
		{
			var c = Select(microService, category, name);

			Update(c.Token, c.Name, c.Folder);
		}

		public void Update(Guid component, string name, Guid folder)
		{
			var c = Select(component) ?? throw new SysException(SR.ErrComponentNotFound);
			c.DemandDevelopmentStage();

			var v = new Validator();

			v.Unique(c, name, nameof(IComponent.Name), QueryByNameSpace(c.MicroService, c.NameSpace));

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			IFolder f = null;

			if (folder != Guid.Empty)
			{
				f = DataModel.Folders.Select(folder);

				if (f is null)
					throw new SysException(SR.ErrFolderNotFound);
			}

			Set(c.Token, new ComponentIndexEntry
			{
				Category = c.Category,
				Name = name,
				Folder = folder,
				MicroService = c.MicroService,
				Modified = DateTime.UtcNow,
				NameSpace = c.NameSpace,
				Token = c.Token,
				Type = c.Type
			}, TimeSpan.Zero);

			FileSystem.Serialize(All());
			CachingNotifications.ComponentChanged(c.MicroService, c.Folder, component, c.NameSpace, c.Category, c.Name);
		}

		public void Delete(Guid component, Guid user)
		{
			var c = Select(component) ?? throw new SysException(SR.ErrComponentNotFound);
			c.DemandDevelopmentStage();

			Remove(component);
			FileSystem.Serialize(All());
			CachingNotifications.ComponentRemoved(c.MicroService, c.Folder, component, c.NameSpace, c.Category, c.Name);
		}

		public string CreateComponentName(Guid microService, string prefix, string nameSpace)
		{
			var existing = Where(f => f.MicroService == microService && string.Compare(nameSpace, f.NameSpace, true) == 0);

			return Shell.GetService<INamingService>().Create(prefix, existing.Select(f => f.Name), true);
		}
	}
}
