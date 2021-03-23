using System;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Design.Ide;
using TomPIT.Ide.ComponentModel;
using TomPIT.Reflection;

namespace TomPIT.Ide.Models
{
	public class AddItemModel
	{
		private string _proposedName = null;

		public IItemDescriptor Descriptor { get; set; }
		public IEnvironment Environment { get; set; }

		public string ProposedName
		{
			get
			{
				if (_proposedName == null)
				{
					if (Environment.Selection.Element == null)
						return _proposedName;

					if (Descriptor.Type.ImplementsInterface<IFolder>())
						_proposedName = ProposedFolderName();
					else
					{
						var category = Descriptor.Id;

						if (string.IsNullOrWhiteSpace(category))
							_proposedName = string.Format("{0}1", Descriptor.Text.Replace(" ", string.Empty));
						else
						{
							var existing = Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(Environment.Selection.Element.MicroService(), category).Select(f => f.Name);
							_proposedName = Environment.Context.Tenant.GetService<INamingService>().Create(Descriptor.Id, existing);
						}
					}
				}

				return _proposedName;
			}
		}

		private string ProposedFolderName()
		{
			IFolder folder = null;

			if (Environment.Selection.Element is IFolder f)
				folder = f;
			else
			{
				var scope = DomQuery.Closest<IFolderScope>(Environment.Selection.Element);

				if (scope != null)
					folder = scope.Folder;
			}

			var ms = DomQuery.Closest<IMicroServiceScope>(Environment.Selection.Element).MicroService.Token;
			var existingFolders = Environment.Context.Tenant.GetService<IComponentService>().QueryFolders(ms, folder == null ? Guid.Empty : folder.Token);
			return Environment.Context.Tenant.GetService<INamingService>().Create("Folder", existingFolders.Select(f1 => f1.Name));
		}
	}
}
