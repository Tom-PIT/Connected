using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace TomPIT.Navigation
{
	internal class NavigationHandlerDescriptor
	{
		private HashSet<string> _keys = null;
		private HashSet<string> _templates = null;
		public NavigationHandlerDescriptor(Guid microService, Guid component, Type handler)
		{
			Component = component;
			Handler = handler;
			MicroService = microService;
		}

		public Guid MicroService { get; }
		public Type Handler { get; }
		public Guid Component { get; }

		public HashSet<string> RouteKeys
		{
			get
			{
				if (_keys == null)
					_keys = new HashSet<string>();

				return _keys;
			}
		}

		public HashSet<string> Templates
		{
			get
			{
				if (_templates == null)
					_templates = new HashSet<string>();

				return _templates;
			}
		}

		public string Match(string url, RouteValueDictionary parameters)
		{
			foreach (var template in Templates)
			{
				var parsedTemplate = TemplateParser.Parse(template);
				var matcher = new TemplateMatcher(parsedTemplate, GetDefaults(parsedTemplate));

				if (matcher.TryMatch(url, parameters))
					return template;
			}

			return null;
		}

		private static RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
		{
			var result = new RouteValueDictionary();

			foreach (var parameter in parsedTemplate.Parameters)
			{
				if (parameter.DefaultValue != null)
					result.Add(parameter.Name, parameter.DefaultValue);
			}

			return result;
		}
	}
}
