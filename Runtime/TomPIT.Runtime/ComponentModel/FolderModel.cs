using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Deployment;

namespace TomPIT.ComponentModel
{
	public class FolderModel
	{
		private List<FolderModel> _folders = null;

		public FolderModel(IFolder folder)
		{
			Folder = folder;
		}

		public IFolder Folder { get; }

		public List<FolderModel> Items
		{
			get
			{
				if (_folders == null)
					_folders = new List<FolderModel>();

				return _folders;
			}
		}

		public static List<FolderModel> Create(Guid microService, List<IPackageFolder> folders)
		{
			var r = new List<FolderModel>();
			var root = folders.Where(f => f.Parent == Guid.Empty);

			foreach (var i in root)
			{
				var m = new FolderModel(FromPackageFolder(microService, i));

				Create(microService, folders.Except(root).ToList(), m);

				r.Add(m);
			}

			return r;
		}

		public static List<FolderModel> Create(ImmutableList<IFolder> folders)
		{
			var r = new List<FolderModel>();
			var root = folders.Where(f => f.Parent == Guid.Empty);

			foreach (var i in root)
			{
				var m = new FolderModel(i);

				Create(folders.Except(root).ToList(), m);

				r.Add(m);
			}

			return r;
		}

		private static void Create(List<IFolder> folders, FolderModel parent)
		{
			var children = folders.Where(f => f.Parent == parent.Folder.Token);

			if (children.Count() == 0)
				return;

			foreach (var i in children)
			{
				var m = new FolderModel(i);

				Create(folders.Except(children).ToList(), m);

				parent.Items.Add(m);
			}
		}

		private static void Create(Guid microService, List<IPackageFolder> folders, FolderModel parent)
		{
			var children = folders.Where(f => f.Parent == parent.Folder.Token);

			if (children.Count() == 0)
				return;

			foreach (var i in children)
			{
				var m = new FolderModel(FromPackageFolder(microService, i));

				Create(microService, folders.Except(children).ToList(), m);

				parent.Items.Add(m);
			}
		}

		private static IFolder FromPackageFolder(Guid microservice, IPackageFolder folder)
		{
			return new Folder
			{
				Name = folder.Name,
				Parent = folder.Parent,
				Token = folder.Token,
				MicroService = microservice
			};
		}

	}
}
