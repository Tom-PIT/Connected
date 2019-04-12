using System;
using TomPIT.Services;

namespace TomPIT.Ide.Design
{
    public class ComponentUpdateArgs : EventArgs
    {
        public ComponentUpdateArgs(EnvironmentMode mode, bool performLock)
        {
            Mode = mode;
            PerformLock = performLock;
        }

        public EnvironmentMode Mode { get; }
        public bool PerformLock { get; }
    }
}
