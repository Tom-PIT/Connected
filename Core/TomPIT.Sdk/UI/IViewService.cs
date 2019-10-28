using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.UI
{
	public enum ViewKind
	{
		Master = 1,
		View = 2,
		Partial = 3,
		MailTemplate = 5,
		Report = 6
	}

	public interface IViewService
	{
		IViewConfiguration Select(string url, ActionContext context);
		//string SelectScripts(Guid microService, Guid view);
		IConfiguration Select(Guid view);
		IMasterViewConfiguration SelectMaster(string name);
		IPartialViewConfiguration SelectPartial(string name);

		string SelectContent(IGraphicInterface ui);

		bool HasChanged(ViewKind kind, string url);

		ViewKind ResolveViewKind(string url);
		IMicroService ResolveMicroService(string url);
	}
}
