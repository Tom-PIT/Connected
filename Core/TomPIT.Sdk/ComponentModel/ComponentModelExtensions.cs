using System;
using System.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Reflection.Manifests;

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

			if (component == null)
				return Guid.Empty;

			return component.MicroService;
		}

		public static string ComponentName(this IConfiguration configuration)
		{
			var c = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(configuration.Component);

			return c == null ? string.Empty : c.Name;
		}

		public static void ValidateMicroServiceReference(this IMicroService service, string reference)
		{
			if (string.IsNullOrWhiteSpace(reference))
				throw new RuntimeException(SR.ErrReferenceMissingSource, string.Format("{0} ({1}->{2})", SR.ErrReferenceMissing, service.Name, "?"));

			if (string.Compare(service.Name, reference, true) == 0)
				return;

			var refs = MiddlewareDescriptor.Current.Tenant.GetService<IDiscoveryService>().References(service.Name);

			if (refs == null || refs.MicroServices.FirstOrDefault(f => string.Compare(f.MicroService, reference, true) == 0) == null)
				throw new RuntimeException(SR.ErrReferenceMissingSource, string.Format("{0} ({1}->{2})", SR.ErrReferenceMissing, service.Name, reference));
		}

		public static Guid ResourceGroup(this IComponent component)
		{
			if (MiddlewareDescriptor.Current.Tenant == null)
				return Guid.Empty;

			var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

			if (ms == null)
				return Guid.Empty;

			return ms.ResourceGroup;
		}

		public static IComponentManifest Manifest(this IComponent component)
		{
			return Manifest(component, Guid.Empty);
		}
		public static IComponentManifest Manifest(this IComponent component, Guid element)
		{
			var config = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token);

			if (config == null)
				return null;

			var att = config.GetType().FindAttribute<ManifestAttribute>();

			if (att == null)
				return null;

			var provider = att.Type == null ?
				Type.GetType(att.TypeName).CreateInstance<IComponentManifestProvider>()
				: att.Type.CreateInstance<IComponentManifestProvider>();

			return provider.CreateManifest(MiddlewareDescriptor.Current.Tenant, component.Token, element);
		}

	}
}
