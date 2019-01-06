using Microsoft.AspNetCore.Mvc.Rendering;

namespace TomPIT
{
	public static class Helper
	{
		public static Renderer TP(this IHtmlHelper @this)
		{
			return new Renderer(@this);
		}
	}
}
