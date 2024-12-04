using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Reflection;

namespace TomPIT.Compilation;
internal class CompilationSet : Queue<IMicroService>
{
	public CompilationSet()
	{
		CompilationFlags = new();
		Initialize();
	}

	private Dictionary<Guid, bool> CompilationFlags { get; }

	public bool ShouldCompile(IMicroService service)
	{
		if (CompilationFlags.TryGetValue(service.Token, out bool result))
			return result;

		return false;
	}
	private void Initialize()
	{
		var microServices = Tenant.GetService<IMicroServiceService>().Query();

		foreach (var microService in microServices)
			Initialize(microService);
	}

	private void Initialize(IMicroService microService)
	{
		var references = Tenant.GetService<IDiscoveryService>().MicroServices.References.References(microService.Token, false);

		foreach (var reference in references)
			Initialize(reference);

		if (!Contains(microService))
		{
			var shouldRecompile = ShouldRecompile(microService);

			Enqueue(microService);

			CompilationFlags.Add(microService.Token, shouldRecompile);
		}
	}

	private bool ShouldRecompile(IMicroService microService)
	{
#if NORECOMPILE
		return false;
#endif

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

			var references = Tenant.GetService<IDiscoveryService>().MicroServices.References.References(microService.Token, false);

			foreach (var reference in references)
			{
				if (CompilationFlags.TryGetValue(reference.Token, out bool shouldRecompileReference) && shouldRecompileReference)
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
