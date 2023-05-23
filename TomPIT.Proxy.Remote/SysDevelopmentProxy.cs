using TomPIT.Proxy.Development;
using TomPIT.Proxy.Remote.Development;

namespace TomPIT.Proxy.Remote
{
    internal class SysDevelopmentProxy : ISysDevelopmentProxy
    {
        public SysDevelopmentProxy()
        {
            Components = new ComponentDevelopmentController();
            Notifications = new DevelopmentNotificationController();
            Folders = new FolderDevelopmentController();
            VersionControl = new VersionControlDevelopmentController();
        }

        public IComponentDevelopmentController Components { get; }
        public IDevelopmentNotificationController Notifications { get; }
        public IFolderDevelopmentController Folders { get; }
        public IVersionControlController VersionControl { get; }
    }
}
