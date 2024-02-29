using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class DevelopmentHandler : IDevelopmentHandler
	{
		private IFolderHandler _folders = null;
		private IComponentHandler _components = null;
		private IQaHandler _qa = null;

		public IQaHandler QA
		{
			get
			{
				if (_qa == null)
					_qa = new QaHandler();

				return _qa;
			}
		}

		public IFolderHandler Folders
		{
			get
			{
				if (_folders == null)
					_folders = new FolderHandler();

				return _folders;
			}
		}

		public IComponentHandler Components
		{
			get
			{
				if (_components == null)
					_components = new ComponentHandler();

				return _components;
			}
		}
	}
}