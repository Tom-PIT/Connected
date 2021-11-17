using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Globalization
{
    public abstract class CultureProviderBase: IRequestCultureProvider
    {
        public static Task<ProviderCultureResult> Unresolved => Task.FromResult<ProviderCultureResult>(null);

        public abstract Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext); 
    }
}
