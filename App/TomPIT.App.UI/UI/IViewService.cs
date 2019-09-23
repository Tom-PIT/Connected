using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.App.UI
{
	internal interface IViewService
	{
		IViewConfiguration Select(string url, ActionContext context);
		//string SelectScripts(Guid microService, Guid view);
		IConfiguration Select(Guid view);
		IMasterViewConfiguration SelectMaster(string name);
		IPartialViewConfiguration SelectPartial(string name);

		string SelectContent(IGraphicInterface ui);

		bool HasChanged(ViewKind kind, string url);
		bool HasSnippetChanged(ViewKind kind, string url);
	}
}
