using System;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Security;

namespace TomPIT.Routing
{
    internal class DesignNotificationService : IDesignNotificationProxy
    {
        public void ConfigurationChanged(Guid component)
        {
            Instance.SysProxy.Components.Refresh(component);

            var cm = Instance.SysProxy.Components.SelectByToken(component);

            Instance.SysProxy.Development.Notifications.ConfigurationChanged(cm.MicroService, cm.Token, cm.Category);

            var componentNotification = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>() as IComponentNotification;

            componentNotification?.NotifyChanged(this, new ComponentEventArgs(cm.MicroService, cm.Folder, cm.Token, cm.NameSpace, cm.Category, cm.Name));
        }

        public void ConfigurationRemoved(Guid component)
        {
            var cm = Instance.SysProxy.Components.SelectByToken(component);

            Instance.SysProxy.Components.Refresh(component);
            Instance.SysProxy.Development.Notifications.ConfigurationRemoved(cm.MicroService, cm.Token, cm.Category);

            var componentNotification = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>() as IComponentNotification;

            componentNotification?.NotifyRemoved(this, new ComponentEventArgs(cm.MicroService, cm.Folder, cm.Token, cm.NameSpace, cm.Category, cm.Name));
        }

        public void ConfigurationAdded(Guid component)
        {
            var cm = Instance.SysProxy.Components.SelectByToken(component);

            Instance.SysProxy.Components.Refresh(component);
            Instance.SysProxy.Development.Notifications.ConfigurationAdded(cm.MicroService, cm.Token, cm.Category);

            var componentNotification = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>() as IComponentNotification;

            componentNotification?.NotifyAdded(this, new ComponentEventArgs(cm.MicroService, cm.Folder, cm.Token, cm.NameSpace, cm.Category, cm.Name));
        }

        public void SourceTextChanged(Guid microService, Guid component, Guid token, int type)
        {
            // not yet implemented — mirrors commented-out body in DebugRouteHandler
        }
    }
}
