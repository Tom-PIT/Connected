using System;
using System.Text;
using TomPIT.Application.UI;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.UI
{
	internal class ViewScriptsCache : ClientRepository<ViewScripts, string>
	{
		public ViewScriptsCache(ISysConnection connection) : base(connection, "viewscript")
		{

		}

		public string Select(Guid microService, Guid view)
		{
			var r = Get(GenerateKey(microService, view));

			if (r != null)
				return r.Content;

			var svc = Instance.GetService<IComponentService>();
			var c = svc.SelectConfiguration(view) as ViewBase;

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			r = new ViewScripts();
			var sb = new StringBuilder();

			foreach (var i in c.Scripts)
			{
				var script = svc.SelectText(microService, i);

				if (!string.IsNullOrWhiteSpace(script))
				{
					sb.Append(script);
					sb.AppendLine();
				}
			}

			r.Content = sb.ToString();

			Set(GenerateKey(microService, view), r);

			return r.Content;
		}

		public void Remove(Guid microService, Guid view)
		{
			Remove(GenerateKey(microService, view));
		}
	}
}
