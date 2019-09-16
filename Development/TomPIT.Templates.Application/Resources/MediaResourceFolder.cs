using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Resources
{
	[Create(DesignUtils.Folder, nameof(Name))]
	public class MediaResourceFolder : ConfigurationElement, IMediaResourceFolder
	{
		private ListItems<IMediaResourceFolder> _folders = null;
		private ListItems<IMediaResourceFile> _files = null;

		[Browsable(false)]
		public ListItems<IMediaResourceFolder> Folders
		{
			get
			{
				if (_folders == null)
					_folders = new ListItems<IMediaResourceFolder> { Parent = this };

				return _folders;
			}
		}
		[Browsable(false)]
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

		[Browsable(false)]
		public DateTime Modified { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
