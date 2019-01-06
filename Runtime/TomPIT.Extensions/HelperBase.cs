using Microsoft.AspNetCore.Mvc.Rendering;

namespace TomPIT
{
	public abstract class HelperBase
	{
		protected HelperBase(IHtmlHelper helper)
		{
			Html = helper;
		}

		protected IHtmlHelper Html { get; private set; }
	}
}
