using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class DevelopmentHandler : IDevelopmentHandler
	{
		private IMicroServiceHandler _microServices = null;
		private IFolderHandler _folders = null;
		private IComponentHandler _components = null;
		private IQaHandler _qa = null;
		private IVersionControlHandler _versionControl = null;
		private IDevelopmentErrorHandler _errors = null;
		private IToolsHandler _tools = null;

		public IDevelopmentErrorHandler Errors
		{
			get
			{
				if (_errors == null)
					_errors = new DevelopmentErrorHandler();

				return _errors;
			}
		}

		public IQaHandler QA
		{
			get
			{
				if (_qa == null)
					_qa = new QaHandler();

				return _qa;
			}
		}

		public IVersionControlHandler VersionControl
		{
			get
			{
				if (_versionControl == null)
					_versionControl = new VersionControlHandler();

				return _versionControl;
			}
		}

		public IMicroServiceHandler MicroServices
		{
			get
			{
				if (_microServices == null)
					_microServices = new MicroServiceHandler();

				return _microServices;
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

		public IToolsHandler Tools
		{
			get
			{
				if (_tools == null)
					_tools = new ToolsHandler();

				return _tools;
			}
		}
	}
}