﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Messaging;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Cdn.Events
{
	internal static class EventHandlers
	{
		private static Lazy<ConcurrentDictionary<string, List<Tuple<Guid, Guid>>>> _handlers = new Lazy<ConcurrentDictionary<string, List<Tuple<Guid, Guid>>>>();

		static EventHandlers()
		{
			var s = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>();

			s.ConfigurationChanged += OnConfigurationChanged;
			s.ConfigurationAdded += OnConfigurationAdded;
			s.ConfigurationRemoved += OnConfigurationRemoved;

			var configs = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().QueryConfigurations(Shell.GetConfiguration<IClientSys>().ResourceGroups, ComponentCategories.EventBinder);

			foreach (var i in configs)
				AddConfiguration(i);
		}

		private static void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (!e.Category.Equals(ComponentCategories.EventBinder))
				return;

			RemoveConfiguration(e.MicroService, e.Component);
			AddConfiguration(e.Component);
		}

		private static void OnConfigurationRemoved(ITenant sender, ConfigurationEventArgs e)
		{
			if (!e.Category.Equals(ComponentCategories.EventBinder))
				return;

			RemoveConfiguration(e.MicroService, e.Component);
		}

		private static void OnConfigurationAdded(ITenant sender, ConfigurationEventArgs e)
		{
			if (!e.Category.Equals(ComponentCategories.EventBinder))
				return;

			AddConfiguration(e.Component);
		}

		private static void AddConfiguration(IConfiguration configuration)
		{
			if (!(configuration is IEventBindingConfiguration eh))
				return;

			var component = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(configuration.Component);

			foreach (var i in eh.Events)
			{
				if (string.IsNullOrWhiteSpace(i.Event))
					continue;

				if (!Handlers.TryGetValue(i.Event.ToLowerInvariant(), out List<Tuple<Guid, Guid>> list))
				{
					list = new List<Tuple<Guid, Guid>>();

					Handlers[i.Event.ToLowerInvariant()] = list;
				}

				if (list.FirstOrDefault(f => f.Item1 == component.MicroService && f.Item2 == component.Token) == null)
					list.Add(new Tuple<Guid, Guid>(component.MicroService, component.Token));
			}
		}

		private static void AddConfiguration(Guid configuration)
		{
			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(configuration) is IEventBindingConfiguration c))
				return;

			AddConfiguration(c);
		}

		private static void RemoveConfiguration(Guid microService, Guid configuration)
		{
			foreach (var i in Handlers)
			{
				if (i.Value.FirstOrDefault(f => f.Item1 == microService && f.Item2 == configuration) != null)
					i.Value.Remove(new Tuple<Guid, Guid>(microService, configuration));
			}
		}

		private static ConcurrentDictionary<string, List<Tuple<Guid, Guid>>> Handlers { get { return _handlers.Value; } }

		public static List<Tuple<Guid, Guid>> Query(string eventName)
		{
			if (!Handlers.TryGetValue(eventName.ToLowerInvariant(), out List<Tuple<Guid, Guid>> items))
				return null;

			return items;
		}
	}
}