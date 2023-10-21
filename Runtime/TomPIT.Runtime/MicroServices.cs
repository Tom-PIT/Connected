using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT;
public static class MicroServices
{
	private static List<Assembly> _microServices;
	private static List<IStartup> _startups;

	public static ImmutableArray<Assembly> Assemblies
	{
		get
		{
			if (_microServices is null)
			{
				_microServices = new();

				var assemblies = AppDomain.CurrentDomain.GetAssemblies();

				foreach (var assembly in assemblies)
				{
					if (assembly.GetCustomAttribute<MicroServiceAttribute>() is not null)
						_microServices.Add(assembly);
				}
			}

			return _microServices.ToImmutableArray();
		}
	}

	public static ImmutableArray<IStartup> Startups
	{
		get
		{
			if (_startups is null)
			{
				_startups = new();

				foreach (var assembly in Assemblies)
				{
					foreach (var type in assembly.GetTypes())
					{
						if (type.IsAssignableTo(typeof(IStartup)))
						{
							if (type.IsAbstract)
								continue;

							var instance = type.CreateInstance<IStartup>();

							if (instance is not null)
								_startups.Add(instance);
						}
					}
				}
			}

			return _startups.ToImmutableArray();
		}
	}
}
