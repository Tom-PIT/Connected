using System;
using System.Linq;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Middleware.Interop;
using TomPIT.Security;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
    public class SiteMapStreamRoute : SiteMapElement, ISiteMapStreamRoute, ISitemapNavigationContextElement
    {
        private ConnectedList<ISiteMapRoute, ISiteMapRoute> _items = null;

        [CIP(CIP.ApiOperationProvider)]
        [AA(AA.ApiOperationAnalyzer)]
        public string Api { get; set; }
        public string RouteKey { get; set; }

        public bool BeginGroup { get; set; }
        public object Parameters { get; set; }
        public string QueryString { get; set; }
        public string Template { get; set; }

        public ConnectedList<ISiteMapRoute, ISiteMapRoute> Routes
        {
            get
            {
                if (_items == null)
                    _items = new ConnectedList<ISiteMapRoute, ISiteMapRoute> { Parent = this };

                return _items;
            }
        }

        [CIP(CIP.NavigationContextProvider)]
        [AA(AA.NavigationContextAnalyzer)]
        public string NavigationContext { get; set; }
        public NavigationContextBehavior NavigationContextBehavior { get; set; } = NavigationContextBehavior.Inherit;
        public bool Authorize(IMiddlewareContext context, Guid user)
        {
            if (string.IsNullOrWhiteSpace(Api))
                return false;

            var descriptor = ComponentDescriptor.Api(context, Api);

            try
            {
                descriptor.Validate();
                descriptor.ValidateConfiguration();

                var op = descriptor.Configuration.Operations.FirstOrDefault(f => string.Compare(descriptor.Element, f.Name, true) == 0);
                var type = op.Middleware(context);
                using var msContext = new MicroServiceContext(descriptor.MicroService);
                var middleware = msContext.CreateMiddleware<IOperation>(type, Shell.HttpContext.ParseArguments(Parameters, QueryString));

                middleware.SetContext(context);
                if (msContext is IElevationContext elevation)
                    elevation.State = ElevationContextState.Revoked;

                context.Tenant.GetService<IAuthorizationService>().AuthorizePolicies(msContext, this);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
