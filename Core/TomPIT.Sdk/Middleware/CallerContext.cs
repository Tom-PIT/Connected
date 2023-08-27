using System;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Middleware.Interop;

namespace TomPIT.Middleware;
public class CallerContext : ICallerContext
{
	private CallerContext()
	{

	}
	public string? Component { get; private set; }

	public string? Method { get; private set; }

	public static ICallerContext? Create(object? sender)
	{
		if (sender is null)
			return null;

		return Create(sender.GetType());
	}
	public static ICallerContext? Create(Type? type)
	{
		if (type is null)
			return null;

		var component = Tenant.GetService<ICompilerService>().ResolveComponent(type);

		if (component is null)
			return null;

		return Create(component, type);
	}

	private static ICallerContext? Create(IComponent component, Type type)
	{

		var ms = Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

		if (ms is null)
			return null;

		var componentName = $"{ms.Name}/{component.Name}";

		if (type.IsAssignableTo(typeof(IOperation)))
			componentName += $"/{type.Name}";

		return new CallerContext
		{
			Component = componentName
		};
	}


	public static ICallerContext? Create(object sender, string method)
	{
		var context = Create(sender) as CallerContext;

		if (context is null)
			return null;

		context.Method = method;

		return context;
	}
}
