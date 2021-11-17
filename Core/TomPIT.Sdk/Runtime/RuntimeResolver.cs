using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Reports;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Runtime
{
    public abstract class RuntimeResolver : IRuntimeResolver
    {
        public IMasterViewConfiguration ResolveMaster(MasterViewResolutionArgs e)
        {
            return OnResolveMaster(e);
        }

        protected virtual IMasterViewConfiguration OnResolveMaster(MasterViewResolutionArgs e)
        {
            return null;
        }

        public IRuntimeUrl ResolveUrl(RuntimeUrlKind kind)
        {
            return OnResolveUrl(kind);
        }

        protected virtual IRuntimeUrl OnResolveUrl(RuntimeUrlKind kind)
        {
            return null;
        }

        public IViewConfiguration ResolveView(ViewResolutionArgs e)
        {
            return OnResolveView(e);
        }

        protected virtual IViewConfiguration OnResolveView(ViewResolutionArgs e)
        {
            return null;
        }

        public IPartialViewConfiguration ResolvePartial(PartialViewResolutionArgs e)
        {
            return OnResolvePartial(e);
        }

        protected virtual IPartialViewConfiguration OnResolvePartial(PartialViewResolutionArgs e)
        {
            return null;
        }

        public IMailTemplateConfiguration ResolveMailTemplate(MailTemplateResolutionArgs e)
        {
            return OnResolveMailTemplate(e);
        }

        protected virtual IMailTemplateConfiguration OnResolveMailTemplate(MailTemplateResolutionArgs e)
        {
            return null;
        }

        public IReportConfiguration ResolveReport(ReportResolutionArgs e)
        {
            return OnResolveReport(e);
        }

        protected virtual IReportConfiguration OnResolveReport(ReportResolutionArgs e)
        {
            return null;
        }
    }
}
