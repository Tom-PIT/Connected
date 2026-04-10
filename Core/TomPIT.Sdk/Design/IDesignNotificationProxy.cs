using System;

namespace TomPIT.Design
{
    public interface IDesignNotificationProxy
    {
        void ConfigurationAdded(Guid component);
        void ConfigurationChanged(Guid component);
        void ConfigurationRemoved(Guid component);
        void SourceTextChanged(Guid microService, Guid component, Guid token, int type);
    }
}
