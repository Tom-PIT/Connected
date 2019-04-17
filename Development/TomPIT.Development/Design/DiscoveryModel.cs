using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.Design
{
    public class DiscoveryModel
    {
        private List<IMicroService> _services = null;

        public DiscoveryModel(IExecutionContext context)
        {
            Context = context;
        }

        private IExecutionContext Context { get; }

        public List<IMicroService> Services
        {
            get
            {
                if (_services == null)
                {
                    _services = new List<IMicroService>();
                    var ms = Context.Connection().GetService<IMicroServiceService>().Query();

                    foreach (var i in ms)
                    {
                        if (!Authorize(i))
                            continue;

                        _services.Add(i);
                    }
                }

                return _services;
            }
        }

        private bool Authorize(IMicroService microService)
        {
            var e = new AuthorizationArgs(Context.GetAuthenticatedUserToken(), Claims.ImplementMicroservice, microService.Token.ToString());

            e.Schema.Empty = EmptyBehavior.Deny;
            e.Schema.Level = AuthorizationLevel.Pessimistic;

            return Context.Connection().GetService<IAuthorizationService>().Authorize(Context, e).Success;
        }
    }
}
