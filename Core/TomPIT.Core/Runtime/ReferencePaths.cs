using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace TomPIT.Runtime;
public static class ReferencePaths
{
	private const string ManagedFileName = "ManagedDependencies.json";
	private const string UnmanagedFileName = "UnmanagedDependencies.json";
	static ReferencePaths()
	{
		var managedFileName = Path.Combine(Shell.MicroServicesFolder, ManagedFileName);
		var unmanagedFileName = Path.Combine(Shell.MicroServicesFolder, UnmanagedFileName);

		if (File.Exists(managedFileName))
		{
			using var fs = new FileStream(managedFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			ManagedCache = JsonSerializer.Deserialize<ConcurrentDictionary<string, List<ReferencePath>>>(fs);
		}
		else
			ManagedCache = new();

		if (File.Exists(unmanagedFileName))
		{
			using var fs = new FileStream(unmanagedFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			try
			{
				UnmanagedCache = JsonSerializer.Deserialize<ConcurrentDictionary<string, List<string>>>(fs);
			}
			catch (Exception ex)
			{

			}
		}
		else
			UnmanagedCache = new();
	}

	private static ConcurrentDictionary<string, List<ReferencePath>> ManagedCache { get; }
	private static ConcurrentDictionary<string, List<string>> UnmanagedCache { get; }

	public static string? ResolveUnmanaged(string assemblyName)
	{
		assemblyName = Path.GetFileNameWithoutExtension(assemblyName);

		if (!UnmanagedCache.TryGetValue(assemblyName, out var existing))
			return null;

		foreach (var path in existing)
		{
			var tokens = path.Split(Path.DirectorySeparatorChar);

			if (tokens.Length < 4)
				continue;

			if (!string.Equals(tokens[^3], "runtimes", StringComparison.Ordinal))
				continue;

			var platform = string.Empty;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				//only x64 is currently supported
				platform = "win-x64";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				platform = "linux-x64";
			}
			else //Currently not supported.
				return null;

			if (string.IsNullOrEmpty(platform))
				continue;

			if (string.Equals(tokens[^2], platform, StringComparison.Ordinal))
				return Path.Combine(path, $"{assemblyName}.dll");
		}

		return null;
	}

	public static string? ResolveManaged(string assemblyName, string? version)
	{
		assemblyName = Path.GetFileNameWithoutExtension(assemblyName);

		if (!ManagedCache.TryGetValue(assemblyName, out List<ReferencePath> existing))
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

	public static void Update(List<string> unmanagedPaths)
	{
		foreach (var path in unmanagedPaths)
		{
			var assemblyName = Path.GetFileNameWithoutExtension(Path.GetFileName(path));
			var folder = Path.GetDirectoryName(path);

			if (UnmanagedCache.TryGetValue(assemblyName, out var cache))
			{
				if (!cache.Contains(folder, StringComparer.OrdinalIgnoreCase))
					cache.Add(folder);
			}
			else
				UnmanagedCache.TryAdd(assemblyName, new List<string> { folder });
		}

		var content = JsonSerializer.Serialize(UnmanagedCache);
		var fileName = Path.GetFullPath(Path.Combine(Shell.MicroServicesFolder, UnmanagedFileName));

		File.WriteAllText(fileName, content);
	}

	public static void Update(List<Assembly> assemblies)
	{
		if (assemblies is null)
			return;

		foreach (var assembly in assemblies)
		{
			var name = new AssemblyName(assembly.FullName);
			var version = name.Version.ToString();

			if (ManagedCache.TryGetValue(name.Name, out List<ReferencePath> existing))
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
				ManagedCache.TryAdd(name.Name, new List<ReferencePath>
				{
					new ReferencePath
					{
						Path=assembly.Location,
						Version=version
					}
				});
			}
		}

		var content = JsonSerializer.Serialize(ManagedCache);
		var fileName = Path.GetFullPath(Path.Combine(Shell.MicroServicesFolder, ManagedFileName));

		File.WriteAllText(fileName, content);
	}

	public static void Update(Assembly assembly)
	{
		Update(new List<Assembly> { assembly });
	}
}
