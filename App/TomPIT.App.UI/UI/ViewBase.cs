using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.UI;
using TomPIT.Models;
using TomPIT.Services;

namespace TomPIT.UI
{
	public abstract class ViewBase<T> : RazorPage<T>
	{
		private ListItems<IViewHelper> _helpers = null;
		private IConfiguration _configuration = null;

		public void Helper(string name)
		{
			Helper(name, null);
		}

		protected Guid ComponentId { get; set; }
		protected string ViewType { get; set; }

		public void Helper(string name, JObject args)
		{
			var helper = Helpers.FirstOrDefault(f => string.Compare(name, f.Name) == 0);

			if (helper == null)
				return;

			e.Connection().GetService<ICompilerService>().Execute(Configuration.MicroService(e.Connection()),
				helper, this, new ViewHelperArguments(e, args, this as RazorPage<IExecutionContext>));
		}

		public H Helper<H>(string name)
		{
			return Helper<H>(name, null);
		}

		public H Helper<H>(string name, JObject args)
		{
			var helper = Helpers.FirstOrDefault(f => string.Compare(name, f.Name) == 0);

			if (helper == null)
				return default(H);

			return (H)e.Connection().GetService<ICompilerService>().Execute(Configuration.MicroService(e.Connection()),
				helper, this, new ViewHelperArguments(e, args, this as RazorPage<IExecutionContext>));
		}

		public IRuntimeViewModel e => Model as IRuntimeViewModel;

		private IConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = Instance.GetService<IComponentService>().SelectConfiguration(ComponentId);

				return _configuration;
			}
		}

		private ListItems<IViewHelper> Helpers
		{
			get
			{
				if (_helpers == null && e != null)
				{
					if (Configuration != null)
					{
						if (string.Compare(ViewType, "partial", true) == 0)
							_helpers = ((IPartialView)Configuration).Helpers;
						else if (string.Compare(ViewType, "view", true) == 0)
							_helpers = ((IView)Configuration).Helpers;
						else if (string.Compare(ViewType, "master", true) == 0)
							_helpers = ((IMasterView)Configuration).Helpers;
						else if (string.Compare(ViewType, "mailtemplate", true) == 0)
							_helpers = ((IMailTemplate)Configuration).Helpers;
					}
				}

				return _helpers;
			}
		}

		protected string GetString(string stringTable, string key)
		{
			return e.Services.Localization.GetString(stringTable, key);
		}
	}
}
