﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.RegularExpressions;

using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT;

public static class Shell
{
	public static event EventHandler ServiceRegistered;
	private static JsonDocument _jsonConfiguration;
	private static IConfigurationRoot _configuration;

	static Shell()
	{
		Container = new ServiceContainer(null);

		ProbingPaths = new List<string>
		{
			typeof(object).Assembly.Location,
			typeof(HttpContext).Assembly.Location
		};

		ValidAssemblyNameExtensions = new[] { ".libdy", ".so", "" };
		LoadedNativeLibraries = new();
		LoadedAssemblies = new();

		AssemblyLoadContext.Default.Resolving += OnResolvingAssembly;
		AssemblyLoadContext.Default.ResolvingUnmanagedDll += OnResolvingUnmanagedDll; ;

		Initialize();
	}


	private static readonly ConfigurationBindings _configurationBinder = new();
	private static ServiceContainer Container { get; }
	private static List<string> ProbingPaths { get; }
	private static ConcurrentDictionary<string, nint> LoadedNativeLibraries { get; }
	public static string InstanceName => _configurationBinder.InstanceName ?? string.Empty;
	public static string MicroServicesFolder => _configurationBinder.MicroServicesFolder ?? "/microServices";
	public static bool LegacyServices => _configurationBinder.LegacyServices;
	public static Version Version => typeof(Shell).Assembly.GetName().Version;
	public static HttpContext HttpContext => Accessor?.HttpContext;
	private static IHttpContextAccessor Accessor { get; set; }
	private static List<string> LoadedAssemblies { get; }
	private static string[] ValidAssemblyNameExtensions { get; }
	private static bool Cleaned { get; set; }

	public static void Initialize()
	{
		Configuration.Bind(_configurationBinder);
	}

	private class ConfigurationBindings
	{
		public string? InstanceName { get; set; }

		public string? MicroServicesFolder { get; set; }
		public bool LegacyServices { get; set; } = true;
	}

	private static nint OnResolvingUnmanagedDll(Assembly requestingAssembly, string libName)
	{
		if (LoadedNativeLibraries.TryGetValue(libName, out var cachedHandle))
			return cachedHandle;

		var path = ResolveUnmanagedAssemblyPath(libName);

		if (string.IsNullOrWhiteSpace(path))
			return 0;

		var handle = NativeLibrary.Load(path, requestingAssembly, null);

		LoadedNativeLibraries.AddOrUpdate(libName, handle, (key, value) => value);

		return handle;
	}

	public static string ResolveUnmanagedAssemblyPath(string assemblyName)
	{
		assemblyName = Path.GetFileNameWithoutExtension(assemblyName);

		if (ReferencePaths.ResolveUnmanaged(assemblyName) is string resolved)
			return resolved;

		var directoriesToCheck = new List<string>();

		//Directories in order

		//Assembly path
		var appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		directoriesToCheck.Add(appPath);

		//Assembly runtimes path
		directoriesToCheck.Add(Path.Combine(appPath, "runtimes", RuntimeInformation.RuntimeIdentifier)); //Specific folder

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			if (RuntimeInformation.OSArchitecture == Architecture.X64)
				directoriesToCheck.Add(Path.Combine(appPath, "runtimes", "linux-x64", "native"));
			else
				directoriesToCheck.Add(Path.Combine(appPath, "runtimes", "linux", "native"));
		}

		//Plugins and their runtimes
		if (Plugins.Items is not null && !string.IsNullOrWhiteSpace(Plugins.Location))
		{
			directoriesToCheck.Add(Plugins.Location);

			directoriesToCheck.Add(Path.Combine(Plugins.Location, "runtimes", RuntimeInformation.RuntimeIdentifier, "native"));

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				if (RuntimeInformation.OSArchitecture == Architecture.X64)
					directoriesToCheck.Add(Path.Combine(Plugins.Location, "runtimes", "linux-x64", "native"));
				else
					directoriesToCheck.Add(Path.Combine(Plugins.Location, "runtimes", "linux", "native"));
			}
		}

		directoriesToCheck.Add(Plugins.Location);

		directoriesToCheck.Add(Path.Combine(appPath, "Plugins", "runtimes", RuntimeInformation.RuntimeIdentifier, "native"));

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			if (RuntimeInformation.OSArchitecture == Architecture.X64)
				directoriesToCheck.Add(Path.Combine(appPath, "Plugins", "runtimes", "linux-x64", "native"));
			else
				directoriesToCheck.Add(Path.Combine(appPath, "Plugins", "runtimes", "linux", "native"));
		}

		//Probing paths
		directoriesToCheck.AddRange(ProbingPaths);

		foreach (var i in directoriesToCheck)
		{
			if (!Directory.Exists(i))
				continue;

			var files = Directory.GetFiles(i);

			foreach (var j in files)
			{
				var name = Path.GetFileNameWithoutExtension(j);

				if (string.Equals(name, assemblyName, StringComparison.OrdinalIgnoreCase) && ValidAssemblyNameExtensions.Contains(Path.GetExtension(j)))
					return j;
			}
		}

		return null;
	}

	public static string ResolveAssemblyPath(string assemblyName)
	{
		if (!assemblyName.EndsWith(".dll"))
			assemblyName = $"{assemblyName}.dll";

		if (TryResolveMicroServicePath(assemblyName, out string result))
			return Path.GetFullPath(result);

		if (TryResolveDependencyPath(assemblyName, out string dependencyPath))
			return dependencyPath;

		if (TryResolveBinPath(assemblyName, out string binPath))
			return Path.GetFullPath(binPath);

		if (TryResolveFrameworkPath(assemblyName, out string frameworkPath))
			return Path.GetFullPath(frameworkPath);

		if (TryResolvePluginPath(assemblyName, out string pluginResult))
			return Path.GetFullPath(pluginResult);

		if (TryResolvePluginsPath(assemblyName, out string pluginsPath))
			return Path.GetFullPath(pluginsPath);

		if (TryResolveProbingPath(assemblyName, out string probingPath))
			return Path.GetFullPath(probingPath);

		return null;
	}

	private static bool TryResolveDependencyPath(string assemblyName, out string result)
	{
		result = ReferencePaths.ResolveManaged(assemblyName, null);

		return result is not null;
	}

	private static bool TryResolveProbingPath(string assemblyName, out string result)
	{
		result = string.Empty;

		foreach (var probingPath in ProbingPaths)
		{
			result = Path.Combine(Path.GetDirectoryName(probingPath), assemblyName);

			if (File.Exists(result))
				return true;
		}

		return false;
	}

	private static bool TryResolveFrameworkPath(string assemblyName, out string result)
	{
		var appPath = RuntimeEnvironment.GetRuntimeDirectory();

		result = Path.Combine(Path.GetDirectoryName(appPath), assemblyName);

		return File.Exists(result);
	}

	private static bool TryResolveBinPath(string assemblyName, out string result)
	{
		var appPath = Assembly.GetEntryAssembly().Location;

		result = Path.Combine(Path.GetDirectoryName(appPath), assemblyName);

		return File.Exists(result);
	}

	private static bool TryResolvePluginsPath(string assemblyName, out string path)
	{
		var appPath = Assembly.GetEntryAssembly().Location;

		path = Path.Combine(Path.GetDirectoryName(appPath), "Plugins", assemblyName);

		return File.Exists(path);
	}

	private static bool TryResolvePluginPath(string assemblyName, out string path)
	{
		path = string.Empty;

		if (Plugins.Items is null || string.IsNullOrWhiteSpace(Plugins.Location))
			return false;

		var dirs = Directory.GetDirectories(Plugins.Location);

		dirs = dirs.Append(Plugins.Location).ToArray();

		foreach (var i in dirs)
		{
			var files = Directory.GetFiles(i);

			foreach (var j in files)
			{
				var name = Path.GetFileNameWithoutExtension(j);
				var asmName = Path.GetFileNameWithoutExtension(assemblyName);

				if (string.Equals(name, asmName, StringComparison.OrdinalIgnoreCase))
				{
					path = j;

					return true;
				}
			}
		}

		return false;
	}
	private static bool TryResolveMicroServicePath(string assemblyName, out string path)
	{
		if (!Directory.Exists(MicroServicesFolder))
			Directory.CreateDirectory(MicroServicesFolder);

		path = Path.Combine(MicroServicesFolder, assemblyName);

		return File.Exists(path);
	}

	private static Assembly OnResolvingAssembly(AssemblyLoadContext ctx, AssemblyName asm)
	{
		var path = ResolveAssemblyPath(asm.Name);

		if (path is null)
			return null;

		if (Plugins.Items is not null && Plugins.ShadowCopy)
		{
			var shadowCopyLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Plugins");

			if (!Cleaned)
			{
				Cleaned = true;

				if (Directory.Exists(shadowCopyLocation))
				{
					var files = Directory.GetFiles(shadowCopyLocation);

					foreach (var i in files)
						File.Delete(i);
				}
			}


			if (!Directory.Exists(shadowCopyLocation))
				Directory.CreateDirectory(shadowCopyLocation);

			var targetPath = Path.Combine(shadowCopyLocation, Path.GetFileName(path));

			if (!LoadedAssemblies.Contains(targetPath))
			{
				LoadedAssemblies.Add(targetPath);

				File.Copy(path, targetPath, true);
			}

			path = targetPath;
		}

		return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
	}

	[DebuggerStepThrough]
	public static T GetService<T>()
	{
		return Container.Get<T>();
	}

	public static void RegisterService(Type contract, object instance)
	{
		Container.Register(contract, instance);

		ServiceRegistered?.Invoke(contract, EventArgs.Empty);
	}

	public static void RegisterService(Type contract, ServiceActivatorCallback callback)
	{
		Container.Register(contract, callback);

		ServiceRegistered?.Invoke(contract, EventArgs.Empty);
	}

	public static IConfigurationRoot Configuration
	{
		get
		{
			if (_configuration is null)
			{
				var appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				var sysPath = Path.Combine(appPath, "sys.json");

				var builder = new ConfigurationBuilder();
				builder.AddJsonFile(sysPath);
				builder.AddEnvironmentVariables();
				builder.AddUserSecrets(Assembly.GetEntryAssembly());

				_configuration = ApplyVariableValues(builder.Build());
			}

			return _configuration;
		}
	}

	private static IConfigurationRoot ApplyVariableValues(IConfigurationRoot configuration)
	{
		var values = configuration.AsEnumerable();

		foreach (var value in values)
		{
			var results = Regex.Matches(value.Value ?? "", @"\$\{(.*?)\}");

			foreach (var match in results.Cast<Match>())
			{
				var key = match.Groups[1].Value;

				if (!configuration.GetSection(key).Exists())
					continue;

				configuration[value.Key] = configuration.GetValue<string>(value.Key)?.Replace($"{match.Value}", configuration.GetValue<string>(key));
			}
		}

		return configuration;
	}

	public static void Configure(IApplicationBuilder app)
	{
		Accessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
	}
}
