using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TomPIT.Annotations;
using TomPIT.Collections;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Connectivity;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Middleware;
internal class MiddlewareService : ConfigurationRepository<IMiddlewareConfiguration>, IMiddlewareService
{
	public MiddlewareService(ITenant tenant) : base(tenant, "middleware")
	{
		Descriptors = new();

		Tenant.GetService<ICompilerService>().Invalidated += OnInvalidateScript;

		Initialize();
	}

	private ConcurrentDictionary<string, List<MiddlewareTypeDescriptor>> Descriptors { get; }
	protected override string[] Categories => new string[] { ComponentCategories.Middleware };
	private void OnInvalidateScript(object sender, Guid e)
	{
		var obsolete = new List<MiddlewareTypeDescriptor>();

		foreach (var descriptor in Descriptors)
		{
			lock (descriptor.Value)
			{
				foreach (var item in descriptor.Value)
				{
					if (item.Type is null)
					{
						obsolete.Add(item);
						continue;
					}

					if (CompilerExtensions.HasScriptReference(item.Type.Assembly, e))
						obsolete.Add(item);
				}
			}
		}

		foreach (var item in obsolete)
			OnChanged(item.MicroService, item.Component);
	}

	protected override void OnChanged(Guid microService, Guid component)
	{
		OnRemoved(microService, component);
		OnAdded(microService, component);
	}

	protected override void OnInitialized()
	{
		Parallel.ForEach(All(), (i) =>
		{
			OnAdded(i.MicroService(), i.Component);
		});
	}

	protected override void OnAdded(Guid microService, Guid component)
	{
		var configuration = Get(component);

		if (configuration is null)
			return;

		var descriptor = new MiddlewareTypeDescriptor(microService, component);

		if (descriptor.Type is null)
			return;

		var targets = MiddlewareExtensions.ResolveImplementedMiddleware(descriptor.Type);

		foreach (var target in targets)
		{
			var key = target.FullName;

			if (Descriptors.TryGetValue(key, out List<MiddlewareTypeDescriptor> items))
				items.Add(descriptor);
			else
				Descriptors.TryAdd(key, new List<MiddlewareTypeDescriptor> { descriptor });
		}
	}

	protected override void OnRemoved(Guid microService, Guid component)
	{
		foreach (var descriptor in Descriptors)
		{
			var targets = descriptor.Value.Where(f => f.Component == component).ToImmutableArray();

			if (targets.Any())
			{
				lock (descriptor.Value)
				{
					foreach (var target in targets)
						descriptor.Value.Remove(target);
				}
			}
		}
	}

	public async Task<TMiddleware?> First<TMiddleware>(IMiddlewareContext context) where TMiddleware : IMiddleware
	{
		return (TMiddleware)(await First(context, typeof(TMiddleware)));
	}

	public async Task<TMiddleware?> First<TMiddleware>(IMiddlewareContext context, ICallerContext? caller) where TMiddleware : IMiddleware
	{
		return (TMiddleware)await First(context, typeof(TMiddleware), caller);
	}

	public async Task<IMiddleware> First(IMiddlewareContext context, Type type)
	{
		return await First(context, type, null);
	}
	public async Task<IMiddleware> First(IMiddlewareContext context, Type type, ICallerContext? caller)
	{
		var items = Prepare(type);

		foreach (var descriptor in items)
		{
			if (!Validate(caller, descriptor.Type))
				continue;

			var instance = await CreateInstance(context, descriptor);

			if (instance is not null && instance.GetType().IsAssignableTo(type))
				return instance;
		}

		return default;
	}

	public async Task<ImmutableList<TMiddleware>> Query<TMiddleware>(IMiddlewareContext context) where TMiddleware : IMiddleware
	{
		return await Query<TMiddleware>(context, null);
	}

	public async Task<ImmutableList<TMiddleware>> Query<TMiddleware>(IMiddlewareContext context, ICallerContext caller) where TMiddleware : IMiddleware
	{
		var items = await Query(context, typeof(TMiddleware), caller);

		if (items is null || !items.Any())
			return ImmutableList<TMiddleware>.Empty;

		var result = new List<TMiddleware>();

		foreach (var item in items)
		{
			if (item is TMiddleware middleware)
				result.Add(middleware);
		}

		return result.ToImmutableList();
	}

	public async Task<ImmutableList<IMiddleware>> Query(IMiddlewareContext context, Type type)
	{
		return await Query(context, type, null);
	}

	public async Task<ImmutableList<IMiddleware>> Query(IMiddlewareContext context, Type type, ICallerContext caller)
	{
		var items = Prepare(type);
		var result = new List<IMiddleware>();

		foreach (var descriptor in items)
		{
			if (!Validate(caller, descriptor.Type))
				continue;

			var instance = await CreateInstance(context, descriptor);

			if (instance is not null && instance.GetType().IsAssignableTo(type))
				result.Add(instance);
		}

		return result.ToImmutableList();
	}

	private async Task<IMiddleware> CreateInstance(IMiddlewareContext context, MiddlewareTypeDescriptor descriptor)
	{
		if (descriptor.Type is null)
			return default;

		try
		{
			using var ctx = new MicroServiceContext(descriptor.MicroService);
			var instance = Tenant.GetService<ICompilerService>().CreateInstance<IMiddleware>(ctx, descriptor.Type);

			if (instance is IMiddlewareObject mi)
				mi.SetContext(context);

			await instance.Initialize();

			return instance;
		}
		catch
		{
			return default;
		}
	}
	private static bool Validate(ICallerContext? context, Type type)
	{
		if (context is null)
			return true;

		var attributes = type.GetCustomAttributes();

		foreach (var attribute in attributes)
		{
			if (attribute.GetType() != typeof(MiddlewareTargetAttribute))
				continue;

			var att = attribute as MiddlewareTargetAttribute;

			if (string.Equals(att.Component, context.Component, StringComparison.Ordinal) && string.Equals(att.Method, context.Method, StringComparison.Ordinal))
				return true;
		}

		return false;
	}

	private ImmutableList<MiddlewareTypeDescriptor> Prepare(Type middlewareType)
	{
		var key = middlewareType.FullName;

		if (key is null || Descriptors is null)
			return ImmutableList<MiddlewareTypeDescriptor>.Empty;

		if (!Descriptors.TryGetValue(key, out List<MiddlewareTypeDescriptor>? items) || items is null)
			return ImmutableList<MiddlewareTypeDescriptor>.Empty;

		var types = new List<Type>();

		foreach (var type in items)
		{
			if (type.Type is null)
				continue;

			types.Add(type.Type);
		}

		if (!types.Any())
			return ImmutableList<MiddlewareTypeDescriptor>.Empty;

		types.SortByPriority();

		var result = new List<MiddlewareTypeDescriptor>();

		foreach (var type in types)
		{
			var descriptor = items.First(f => f.Type == type);

			if (descriptor is not null)
				result.Add(descriptor);
		}

		return result.ToImmutableList();
	}
}
