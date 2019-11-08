using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.MicroServices.Design;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Resources
{
	[DomDesigner(DesignUtils.MediaDesigner, Mode = EnvironmentMode.Design)]
	public class MediaResources : ComponentConfiguration, IMediaResourcesConfiguration
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
	}
}