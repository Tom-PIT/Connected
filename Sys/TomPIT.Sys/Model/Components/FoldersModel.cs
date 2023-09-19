using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Components
{
	public class FoldersModel : SynchronizedRepository<IFolder, Guid>
	{
		public FoldersModel(IMemoryCache container) : base(container, "folder")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Development.Folders.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Development.Folders.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IFolder Select(Guid microService, string name)
		{
			var r = Get(f => f.MicroService == microService && string.Compare(name, f.Name, true) == 0);

			if (r != null)
				return r;

			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			r = Shell.GetService<IDatabaseService>().Proxy.Development.Folders.Select(s, name);

			if (r != null)
				Set(r.Token, r, TimeSpan.Zero);
			return r;
		}

		public IFolder Select(Guid token)
		{
			return Get(token,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					return Shell.GetService<IDatabaseService>().Proxy.Development.Folders.Select(token);
				});
		}

		public ImmutableList<IFolder> Query()
		{
			return All();
		}

		public ImmutableList<IFolder> Query(List<string> resourceGroups)
		{
			var rgs = resourceGroups.ToResourceGroupList();
			var ms = DataModel.MicroServices.Query(rgs);

			return Where(f => ms.Any(t => t.Token == f.MicroService));
		}

		public ImmutableList<IFolder> Query(Guid microService)
		{
			return Where(f => f.MicroService == microService);
		}

		public ImmutableList<IFolder> Query(Guid microService, Guid parent)
		{
			return Where(f => f.MicroService == microService && f.Parent == parent);
		}

		public Guid Insert(Guid microService, string name, Guid parent)
		{
			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var v = new Validator();

			v.Unique(null, name, nameof(IFolder.Name), Query(microService, parent));

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			IFolder p = null;

			if (parent != Guid.Empty)
			{
				p = Select(parent);

				if (p == null)
					throw new SysException(SR.ErrFolderNotFound);
			}

			var token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Development.Folders.Insert(s, name, token, p);

			Refresh(token);
			CachingNotifications.FolderChanged(microService, token);

			return token;
		}

		public void Restore(Guid microService, Guid token, string name, Guid parent)
		{
			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var v = new Validator();

			v.Unique(null, name, nameof(IFolder.Name), Query(microService, parent));

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			IFolder p = null;

			if (parent != Guid.Empty)
			{
				p = Select(parent);

				if (p == null)
					throw new SysException(SR.ErrFolderNotFound);
			}

			Shell.GetService<IDatabaseService>().Proxy.Development.Folders.Insert(s, name, token, p);

			Refresh(token);
		}

		public void Update(Guid microService, Guid folder, string name, Guid parent)
		{
			if (folder == parent)
				throw new SysException(SR.ErrFolderSelf);

			var f = Select(folder);

			if (f == null)
				throw new SysException(SR.ErrFolderNotFound);

			var v = new Validator();

			v.Unique(f, name, nameof(IFolder.Name), Query(microService, parent));

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			IFolder p = null;

			if (parent != Guid.Empty)
			{
				p = Select(parent);

				if (p == null)
					throw new SysException(SR.ErrFolderNotFound);
			}

			Shell.GetService<IDatabaseService>().Proxy.Development.Folders.Update(f, name, p);

			Refresh(folder);
			CachingNotifications.FolderChanged(microService, folder);
		}

		public void Delete(Guid microService, Guid folder)
		{
			var f = Select(folder);

			if (f == null)
				throw new SysException(SR.ErrFolderNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Development.Folders.Delete(f);

			Remove(folder);
			CachingNotifications.FolderRemoved(microService, folder);
		}
	}
}
