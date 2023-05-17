using TomPIT.Proxy.Development;
using TomPIT.Proxy.Local.Development;

namespace TomPIT.Proxy.Local
{
	internal class SysDevelopmentProxy : ISysDevelopmentProxy
	{
		public SysDevelopmentProxy()
		{
			Components = new ComponentDevelopmentController();
			Notifications = new DevelopmentNotificationController();
			Folders = new FolderDevelopmentController();
		}
		public IComponentDevelopmentController Components { get; }

		public IDevelopmentNotificationController Notifications { get; }

		public IFolderDevelopmentController Folders { get; }
	}
}
