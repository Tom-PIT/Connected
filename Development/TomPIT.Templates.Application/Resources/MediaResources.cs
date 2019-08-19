using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	[DomDesigner("TomPIT.Application.Design.Designers.MediaDesigner, TomPIT.Application.Design", Mode = Services.EnvironmentMode.Design)]
	public class MediaResources : ComponentConfiguration, IMediaResources
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
	}
}