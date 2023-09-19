using TomPIT.Environment;
using TomPIT.Runtime;

namespace TomPIT.Middleware
{
    internal class MiddlewareEnvironment : IMiddlewareEnvironment
    {
        public MiddlewareEnvironment()
        {
            var service = Shell.GetService<IRuntimeService>();

            IsInteractive = service.Mode == EnvironmentMode.Runtime && (service.SupportsUI || service.Features.HasFlag(InstanceFeatures.Rest));

        }
        public bool IsInteractive { get; set; } = true;
    }
}
