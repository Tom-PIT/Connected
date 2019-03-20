using Microsoft.AspNetCore.Mvc;
using System;
using TomPIT.ComponentModel.UI;

namespace TomPIT.UI
{
	internal interface IViewService
	{
		IView Select(string url, ActionContext context);
		string SelectScripts(Guid microService, Guid view);
		IGraphicInterface Select(Guid view);
		IMasterView SelectMaster(string name);
		IPartialView SelectPartial(string name);

		string SelectContent(IGraphicInterface ui);

		bool HasChanged(ViewKind kind, string url, ActionContext context);
	}
}
