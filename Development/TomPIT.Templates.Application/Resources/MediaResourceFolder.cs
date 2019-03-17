using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	[Create("Folder", nameof(Name))]
	public class MediaResourceFolder : ConfigurationElement, IMediaResourceFolder
	{
		private ListItems<IMediaResourceFolder> _folders = null;
		private ListItems<IMediaResourceFile> _files = null;

		[Items("TomPIT.Application.Design.Items.MediaResourceFoldersCollection, TomPIT.Application.Design")]
		public ListItems<IMediaResourceFolder> Folders
		{
			get
			{
				if (_folders == null)
					_folders = new ListItems<IMediaResourceFolder> { Parent = this };

				return _folders;
			}
		}

		[Items("TomPIT.Application.Design.Items.MediaResourceFilesCollection, TomPIT.Application.Design")]
		public ListItems<IMediaResourceFile> Files
		{
			get
			{
				if (_files == null)
					_files = new ListItems<IMediaResourceFile> { Parent = this };

				return _files;
			}
		}

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		public string Name { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
