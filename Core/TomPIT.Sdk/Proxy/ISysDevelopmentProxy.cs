using TomPIT.Proxy.Development;

namespace TomPIT.Proxy
{
    public interface ISysDevelopmentProxy
    {
        IComponentDevelopmentController Components { get; }
        IDevelopmentNotificationController Notifications { get; }
        IFolderDevelopmentController Folders { get; }
        IVersionControlController VersionControl { get; }
    }
}
