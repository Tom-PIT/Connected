using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Sys.Notifications;
using TomPIT.Sys.SourceFiles;

namespace TomPIT.Sys.Model.Components
{
	public class FoldersModel : SynchronizedRepository<IFolder, Guid>
	{
		public FoldersModel(IMemoryCache container) : base(container, "folder")
		{
		}

		protected override void OnInitializing()
		{
			var ds = FileSystem.LoadFolders();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		public IFolder Select(Guid microService, string name)
		{
			return Get(f => f.MicroService == microService && string.Equals(name, f.Name, StringComparison.OrdinalIgnoreCase));
		}

		public IFolder Select(Guid token)
		{
			return Get(token);
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
			var s = DataModel.MicroServices.Select(microService) ?? throw new SysException(SR.ErrMicroServiceNotFound);
			var v = new Validator();

			v.Unique(null, name, nameof(IFolder.Name), Query(microService, parent));

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			IFolder p = null;

			if (parent != Guid.Empty)
			{
				p = Select(parent);

				if (p is null)
					throw new SysException(SR.ErrFolderNotFound);
			}

			var token = Guid.NewGuid();

			Set(token, new FolderIndexEntry
			{
				MicroService = microService,
				Parent = parent,
				Name = name,
				Token = token
			}, TimeSpan.Zero);

			FileSystem.Serialize(All());
			CachingNotifications.FolderChanged(microService, token);

			return token;
		}

		public void Restore(Guid microService, Guid token, string name, Guid parent)
		{
			var s = DataModel.MicroServices.Select(microService) ?? throw new SysException(SR.ErrMicroServiceNotFound);
			var v = new Validator();

			v.Unique(null, name, nameof(IFolder.Name), Query(microService, parent));

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			IFolder p = null;

			if (parent != Guid.Empty)
			{
				p = Select(parent);

				if (p is null)
					throw new SysException(SR.ErrFolderNotFound);
			}

			Set(token, new FolderIndexEntry
			{
				MicroService = microService,
				Parent = parent,
				Name = name,
				Token = token
			}, TimeSpan.Zero);

			FileSystem.Serialize(All());
		}

		public void Update(Guid microService, Guid folder, string name, Guid parent)
		{
			if (folder == parent)
				throw new SysException(SR.ErrFolderSelf);

			var f = Select(folder) ?? throw new SysException(SR.ErrFolderNotFound);
			var v = new Validator();

			v.Unique(f, name, nameof(IFolder.Name), Query(microService, parent));

			if (!v.IsValid)
				throw new SysException(v.ErrorMessage);

			IFolder p = null;

			if (parent != Guid.Empty)
			{
				p = Select(parent);

				if (p is null)
					throw new SysException(SR.ErrFolderNotFound);
			}

			Set(f.Token, new FolderIndexEntry
			{
				MicroService = microService,
				Token = folder,
				Name = name,
				Parent = parent,

			}, TimeSpan.Zero);

			FileSystem.Serialize(All());
			CachingNotifications.FolderChanged(microService, folder);
		}

		public void Delete(Guid microService, Guid folder)
		{
			_ = Select(folder) ?? throw new SysException(SR.ErrFolderNotFound);

			Remove(folder);
			FileSystem.Serialize(All());
			CachingNotifications.FolderRemoved(microService, folder);
		}
	}
}
