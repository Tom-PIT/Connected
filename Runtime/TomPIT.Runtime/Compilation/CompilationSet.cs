using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Runtime;

namespace TomPIT.Compilation;

internal enum CompilationMode
{
	CompileIfNewer = 1,
	CompileAlways = 2
}
internal class CompilationSet : Queue<CompilationDescriptor>
{
	public CompilationSet(CompilationMode mode)
	{
		Mode = mode;

		Bootstrappers = [];
		Initialize();
	}

	private CompilationMode Mode { get; }
	private Queue<CompilationDescriptor> Bootstrappers { get; }

	public List<IMicroService> Query()
	{
		var result = new List<IMicroService>();

		foreach (var boot in Bootstrappers)
			result.Add(boot.MicroService);

		foreach (var ms in this)
			result.Add(ms.MicroService);

		return result;
	}
	public bool TryDequeueBootstrapper(out CompilationDescriptor? result)
	{
		return Bootstrappers.TryDequeue(out result);
	}
	private void Initialize()
	{
		var microServices = Tenant.GetService<IMicroServiceService>().Query();

		foreach (var microService in microServices)
			Initialize(microService);
	}

	private void Initialize(IMicroService microService)
	{
		IServiceReferencesConfiguration? configuration = SelectReferenceConfiguration(microService.Token);

		if (configuration is not null)
		{
			foreach (var m in configuration.MicroServices)
			{
				if (string.IsNullOrWhiteSpace(m.MicroService))
					continue;

				var ms = Tenant.GetService<IMicroServiceService>().Select(m.MicroService) ?? throw new NullReferenceException($"Referenced microservice is not registered ('{m.Id}')");

				Initialize(ms);
			}
		}

		if (this.FirstOrDefault(f => f.MicroService.Token == microService.Token) is not null ||
			Bootstrappers.FirstOrDefault(f => f.MicroService.Token == microService.Token) is not null)
			return;

		var isSupported = Tenant.GetService<IRuntimeService>().IsMicroServiceSupported(microService.Token);
		var shouldRecompile = isSupported && ShouldRecompile(microService);
		var descriptor = new CompilationDescriptor
		{
			MicroService = microService,
			ShouldRecompile = shouldRecompile,
			IsSupported = isSupported
		};

		if (configuration is not null && !configuration.Packages.Any() && !configuration.MicroServices.Any())
			Bootstrappers.Enqueue(descriptor);
		else
			Enqueue(descriptor);
	}

	internal static IServiceReferencesConfiguration? SelectReferenceConfiguration(Guid microService)
	{
		var referenceComponent = Tenant.GetService<IComponentService>().SelectComponent(microService, ComponentCategories.Reference, ComponentCategories.ReferenceComponentName);

		if (referenceComponent is null)
			return null;

		return Tenant.GetService<IComponentService>().SelectConfiguration(referenceComponent.Token) as IServiceReferencesConfiguration;
	}

	private bool ShouldRecompile(IMicroService microService)
	{
#if NORECOMPILE
		return false;
#endif

		if (Mode == CompilationMode.CompileAlways)
			return true;

		if (string.IsNullOrEmpty(microService.Version))
			return true;

		var path = Shell.ResolveAssemblyPath(MicroServiceCompiler.ParseAssemblyName(microService));

		if (string.IsNullOrEmpty(path))
			return true;

		var msVersion = Version.Parse(microService.Version);

		try
		{
			var version = AssemblyName.GetAssemblyName(path).Version;

			if (version is null)
				return true;

			if (version != msVersion)
				return true;

			var references = SelectReferenceConfiguration(microService.Token);

			if (references is null)
				return true;

			foreach (var reference in references.MicroServices)
			{
				if (string.IsNullOrWhiteSpace(reference.MicroService))
					continue;

				var ms = Bootstrappers.FirstOrDefault(f => string.Equals(f.MicroService.Name, reference.MicroService, StringComparison.OrdinalIgnoreCase));

				if (ms is not null && ms.ShouldRecompile)
					return true;
			}
		}
		catch
		{
			return true;
		}

		return false;
	}
}
