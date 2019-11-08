using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Navigation
{
	public abstract class SiteMapMiddleware : MiddlewareComponent, ISiteMapHandler
	{
		public List<ISiteMapContainer> Invoke(params string[] keys)
		{
			var r = new List<ISiteMapContainer>();

			if (keys.Length == 0)
			{
				var items = OnInvoke(null);

				if (items != null && items.Count > 0)
					r.AddRange(items);
			}
			else
			{
				foreach (var key in keys)
				{
					var items = OnInvoke(key);

					if (items != null && items.Count > 0)
						r.AddRange(items);
				}
			}

			return r;
		}

		protected virtual List<ISiteMapContainer> OnInvoke(string key)
		{
			return null;
		}
	}
}
