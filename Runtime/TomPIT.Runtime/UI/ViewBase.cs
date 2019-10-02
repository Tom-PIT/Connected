using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json.Linq;
using TomPIT.Collections;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.UI;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Serialization;
using TomPIT.UI;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Runtime.UI
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

			ViewModel.Tenant.GetService<ICompilerService>().Execute(Configuration.MicroService(),
				helper, this, new ViewHelperArguments(ViewModel, args, this as RazorPage<IViewModel>));
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

			return (H)ViewModel.Tenant.GetService<ICompilerService>().Execute(Configuration.MicroService(),
				helper, this, new ViewHelperArguments(ViewModel, args, this as RazorPage<IViewModel>));
		}

		private IViewModel ViewModel => Model as IViewModel;
		private IConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(ComponentId);

				return _configuration;
			}
		}

		private ListItems<IViewHelper> Helpers
		{
			get
			{
				if (_helpers == null && ViewModel != null)
				{
					if (Configuration != null)
					{
						if (string.Compare(ViewType, "partial", true) == 0)
							_helpers = ((IPartialViewConfiguration)Configuration).Helpers;
						else if (string.Compare(ViewType, "view", true) == 0)
							_helpers = ((IViewConfiguration)Configuration).Helpers;
						else if (string.Compare(ViewType, "master", true) == 0)
							_helpers = ((IMasterViewConfiguration)Configuration).Helpers;
						else if (string.Compare(ViewType, "mailtemplate", true) == 0)
							_helpers = ((IMailTemplateConfiguration)Configuration).Helpers;
					}
				}

				return _helpers;
			}
		}

		protected string GetString([CIP(CIP.StringTableProvider)]string stringTable, [CIP(CIP.StringTableStringProvider)]string key)
		{
			return ViewModel.Services.Globalization.GetString(stringTable, key);
		}

		protected string ToJsonString(object content)
		{
			if (content == null)
				return null;

			return Serializer.Serialize(content);
		}
	}
}