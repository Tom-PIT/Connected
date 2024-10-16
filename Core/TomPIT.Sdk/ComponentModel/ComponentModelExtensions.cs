﻿using System;
using System.Linq;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.ComponentModel
{
	public static class ComponentModelExtensions
	{
		public static IConfiguration Configuration(this IElement element)
		{
			return element.Closest<IConfiguration>();
		}

		public static IComponent Component(this IConfiguration configuration)
		{
			return MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(configuration.Component);
		}

		public static Guid MicroService(this IConfiguration configuration)
		{
			var component = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(configuration.Component);

			return component?.MicroService ?? default;
		}

		public static string ComponentName(this IConfiguration configuration)
		{
			var c = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(configuration.Component);

			return c?.Name ?? string.Empty;
		}

		public static void ValidateMicroServiceReference(this IMicroService service, string reference)
		{
			if (string.IsNullOrWhiteSpace(reference))
				throw new RuntimeException(SR.ErrReferenceMissingSource, string.Format("{0} ({1}->{2})", SR.ErrReferenceMissing, service.Name, "?"));

			if (string.Compare(service.Name, reference, true) == 0)
				return;

			var refs = MiddlewareDescriptor.Current.Tenant.GetService<IDiscoveryService>().MicroServices.References.Select(service.Name);

			if (refs is null || refs.MicroServices.FirstOrDefault(f => string.Compare(f.MicroService, reference, true) == 0) is null)
				throw new RuntimeException(SR.ErrReferenceMissingSource, string.Format("{0} ({1}->{2})", SR.ErrReferenceMissing, service.Name, reference));
		}

		public static Guid ResourceGroup(this IComponent component)
		{
			if (MiddlewareDescriptor.Current.Tenant is null)
				return Guid.Empty;

			var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

			if (ms is null)
				return Guid.Empty;

			return ms.ResourceGroup;
		}
	}
}
