﻿using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Services;

namespace TomPIT.UI
{
	public abstract class ViewBase<T> : RazorPage<T>
	{
		private IExecutionContext _context = null;
		private ListItems<IViewHelper> _helpers = null;

		public void Helper(string name)
		{
			Helper(name, null);
		}

		protected Guid ComponentId { get; set; }
		protected string ViewType { get; set; }

		public void Helper(string name, JObject e)
		{
			var helper = Helpers.FirstOrDefault(f => string.Compare(name, f.Name) == 0);

			if (helper == null)
				return;

			ApplicationContext.Connection().GetService<ICompilerService>().Execute(ApplicationContext.MicroService(),
				helper, this, new ViewHelperArguments(ApplicationContext, e, this as ViewBase<IExecutionContext>));
		}

		public H Helper<H>(string name)
		{
			return Helper<H>(name, null);
		}

		public H Helper<H>(string name, JObject e)
		{
			var helper = Helpers.FirstOrDefault(f => string.Compare(name, f.Name) == 0);

			if (helper == null)
				return default(H);

			return (H)ApplicationContext.Connection().GetService<ICompilerService>().Execute(ApplicationContext.MicroService(),
				helper, this, new ViewHelperArguments(ApplicationContext, e, this as ViewBase<IExecutionContext>));
		}

		public IExecutionContext ApplicationContext
		{
			get
			{
				if (_context == null)
					_context = Model as IExecutionContext;

				return _context;
			}
		}

		private ListItems<IViewHelper> Helpers
		{
			get
			{
				if (_helpers == null && ApplicationContext != null)
				{
					var config = Instance.GetService<IComponentService>().SelectConfiguration(ComponentId);

					if (config != null)
					{
						if (string.Compare(ViewType, "partial", true) == 0)
							_helpers = ((IPartialView)config).Helpers;
						else if (string.Compare(ViewType, "view", true) == 0)
							_helpers = ((IView)config).Helpers;
						else if (string.Compare(ViewType, "master", true) == 0)
							_helpers = ((IMasterView)config).Helpers;
					}
				}

				return _helpers;
			}
		}
	}
}