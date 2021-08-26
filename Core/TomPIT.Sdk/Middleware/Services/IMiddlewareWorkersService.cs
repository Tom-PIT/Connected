using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CI = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
    public interface IMiddlewareWorkersService
    {
        void Run([CI(CI.WorkersProvider)]string worker);
    }
}
