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
    public interface IRuntimeResolver
    {
        IRuntimeUrl ResolveUrl(RuntimeUrlKind kind);

        IMasterViewConfiguration ResolveMaster(MasterViewResolutionArgs e);

        IViewConfiguration ResolveView(ViewResolutionArgs e);
        IPartialViewConfiguration ResolvePartial(PartialViewResolutionArgs e);

        IMailTemplateConfiguration ResolveMailTemplate(MailTemplateResolutionArgs e);

        IReportConfiguration ResolveReport(ReportResolutionArgs e);
    }
}
