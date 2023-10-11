using System;
using System.Text;
using TomPIT.Annotations.Design;
using TomPIT.Compilation.Views;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Reports;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Reflection;
using TomPIT.Storage;
using TomPIT.UI;

namespace TomPIT.Compilation;
internal static class ViewCompiler
{
	public static string Compile(ITenant tenant, IText sourceCode)
	{
		var config = sourceCode.Configuration();
		var cmp = tenant.GetService<IComponentService>().SelectComponent(config.Component);

		if (cmp is null)
			return null;

		var rendererAtt = config.GetType().FindAttribute<ViewRendererAttribute>();
		var content = string.Empty;

		if (rendererAtt is not null)
		{
			var renderer = (rendererAtt.Type ?? TypeExtensions.GetType(rendererAtt.TypeName)).CreateInstance<IViewRenderer>();

			content = renderer.CreateContent(tenant, cmp);
		}
		else if (sourceCode.TextBlob != Guid.Empty)
		{
			var r = Tenant.GetService<IStorageService>().Download(sourceCode.TextBlob);

			if (r is null)
				return null;

			content = Encoding.UTF8.GetString(r.Content);
		}

		ProcessorBase processor = null;

		if (config is IMasterViewConfiguration master)
			processor = new MasterProcessor(master, content);
		else if (config is IViewConfiguration view)
			processor = new ViewProcessor(view, content);
		else if (config is IPartialViewConfiguration partial)
			processor = new PartialProcessor(partial, content);
		else if (config is IMailTemplateConfiguration mail)
			processor = new MailTemplateProcessor(mail, content);
		else if (config is IReportConfiguration report)
			processor = new ReportProcessor(report);

		if (processor == null)
			return null;

		processor.Compile(tenant, cmp, config);

		return processor.Result;
	}
}
