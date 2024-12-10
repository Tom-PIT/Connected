using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Collections;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT;
public static class MicroServices
{
	private static List<Assembly> _assemblies;
	private static List<IStartup> _startups;
	static MicroServices()
	{
		_startups = [];
		_assemblies = [];
	}

	public static ImmutableArray<IStartup> Startups
	{
		get
		{
			return !_startups.Any() ? default : ([.. _startups]);
		}
	}
	public static ImmutableArray<Assembly> Assemblies => [.. _assemblies];

	internal static void Register(Assembly assembly)
	{
		var attribute = assembly.GetCustomAttribute<MicroServiceAttribute>();

		if (attribute is null)
			return;

		_assemblies.Add(assembly);

		foreach (var type in assembly.GetTypes())
		{
			if (type.IsAssignableTo(typeof(IStartup)))
			{
				if (type.IsAbstract)
					continue;

				var instance = type.CreateInstance<IStartup>();

				if (instance is not null)
					_startups.Add(instance);

				_startups.SortByPriority();
			}
		}
	}
}
