using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.Navigation
{
	public abstract class SiteMapHandler : ProcessHandler, ISiteMapHandler
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

			return r;
		}

		protected virtual List<ISiteMapContainer> OnInvoke(string key)
		{
			return null;
		}
	}
}
