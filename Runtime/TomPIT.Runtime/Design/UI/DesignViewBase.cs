using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TomPIT.Models;
using TomPIT.Runtime.UI;

namespace TomPIT.Runtime.Design.UI
{
	public abstract class DesignViewBase : ViewBase<IViewModel>
	{
		[RazorInject]
		public IViewComponentHelper Component
		{
			get;
			private set;
		}

		[RazorInject]
		public IHtmlHelper<IViewModel> Html
		{
			get;
			private set;
		}

		[RazorInject]
		public IJsonHelper Json
		{
			get;
			private set;
		}

		[RazorInject]
		public IModelExpressionProvider ModelExpressionProvider
		{
			get;
			private set;
		}

		[RazorInject]
		public IUrlHelper Url
		{
			get;
			private set;
		}
	}
}
