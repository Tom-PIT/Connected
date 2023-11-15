using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace TomPIT.Runtime;
public static class ReferencePaths
{
	static ReferencePaths()
	{
		var fileName = Path.Combine(Shell.MicroServicesFolder, "deps.json");

		if (File.Exists(fileName))
		{
			using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			Cache = JsonSerializer.Deserialize<ConcurrentDictionary<string, List<ReferencePath>>>(fs);
		}
		else
			Cache = new();
	}
	private static ConcurrentDictionary<string, List<ReferencePath>> Cache { get; }

	public static string? Resolve(string name, string? version)
	{
		name = Path.GetFileNameWithoutExtension(name);

		if (!Cache.TryGetValue(name, out List<ReferencePath> existing))
			return null;

		if (version is null)
			return Latest(existing);

		var exact = existing.FirstOrDefault(f => string.Equals(f.Version, version, System.StringComparison.Ordinal));

		if (exact is not null)
			return exact.Path;

		return Latest(existing);
	}

	private static string? Latest(List<ReferencePath> versions)
	{
		ReferencePath? result = null;
		Version? latestVersion = null;

		foreach (var item in versions)
		{
			if (Version.TryParse(item.Version, out Version? version))
			{
				if (latestVersion is null)
				{
					result = item;
					latestVersion = version;
				}
				else if (version > latestVersion)
				{
					result = item;
					latestVersion = version;
				}
			}
		}

		return result?.Path;
	}

	public static void Update(List<Assembly> assemblies)
	{
		if (assemblies is null)
			return;

		foreach (var assembly in assemblies)
		{
			var name = new AssemblyName(assembly.FullName);
			var version = name.Version.ToString();

			if (Cache.TryGetValue(name.Name, out List<ReferencePath> existing))
			{
				var target = existing.FirstOrDefault(f => string.Equals(f.Version, version, System.StringComparison.Ordinal));

				if (target is null)
				{
					existing.Add(new ReferencePath
					{
						Path = assembly.Location,
						Version = version
					});
				}
				else
					target.Path = assembly.Location;
			}
			else
			{
				Cache.TryAdd(name.Name, new List<ReferencePath>
				{
					new ReferencePath
					{
						Path=assembly.Location,
						Version=version
					}
				});
			}
		}

		var content = JsonSerializer.Serialize(Cache);
		var fileName = Path.GetFullPath(Path.Combine(Shell.MicroServicesFolder, "deps.json"));

		File.WriteAllText(fileName, content);
	}

	public static void Update(Assembly assembly)
	{
		Update(new List<Assembly> { assembly });
	}
}
