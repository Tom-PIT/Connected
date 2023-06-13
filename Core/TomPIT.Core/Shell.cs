using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;

namespace TomPIT
{
	public static class Shell
	{
		public static event EventHandler ServiceRegistered;

		private static ServiceContainer _sm = null;
		private static JsonDocument _configuration = null;
		private static IHttpContextAccessor _accessor = null;
		private static Version _version = null;
		private static bool _cleaned = false;
		private static Lazy<List<string>> _loadedAssemblies = new Lazy<List<string>>();
		private static readonly List<string> ProbingPaths;
		static Shell()
		{
			_sm = new ServiceContainer(null);

			ProbingPaths = new List<string>
			{
				typeof(object).Assembly.Location,
				typeof(HttpContext).Assembly.Location
			};

			AssemblyLoadContext.Default.Resolving += OnResolvingAssembly;

			Initialize();
		}

		public static string InstanceName { get; private set; }
		public static Version Version => _version ??= typeof(Shell).Assembly.GetName().Version;
		public static HttpContext HttpContext { get { return _accessor?.HttpContext; } }
		private static List<string> LoadedAssemblies => _loadedAssemblies.Value;

		public static void Initialize()
		{
			if (Configuration.RootElement.TryGetProperty("instanceName", out JsonElement element))
				InstanceName = element.GetString();
		}

		public static string ResolveAssemblyPath(string assemblyName)
		{
			if (assemblyName.EndsWith(".dll"))
				assemblyName = Path.GetFileNameWithoutExtension(assemblyName);

			var appPath = Assembly.GetEntryAssembly().Location;
			var target = Path.Combine(Path.GetDirectoryName(appPath), string.Format("{0}.dll", assemblyName));

			if (Plugins.Items is not null && !string.IsNullOrWhiteSpace(Plugins.Location))
			{
				var dirs = Directory.GetDirectories(Plugins.Location);

				foreach (var i in dirs)
				{
					var files = Directory.GetFiles(i);

					foreach (var j in files)
					{
						var name = Path.GetFileNameWithoutExtension(j);

						if (string.Compare(name, assemblyName, true) == 0)
							return j;
					}
				}
			}

			if (File.Exists(target))
				return target;

			target = Path.Combine(Path.GetDirectoryName(appPath), "Plugins", string.Format("{0}.dll", assemblyName));

			if (File.Exists(target))
				return target;

			foreach (var probingPath in ProbingPaths)
			{
				target = Path.Combine(Path.GetDirectoryName(probingPath), string.Format("{0}.dll", assemblyName));

				if (File.Exists(target))
					return target;
			}

			return null;
		}

		private static Assembly OnResolvingAssembly(AssemblyLoadContext ctx, AssemblyName asm)
		{
			Console.WriteLine(asm.FullName);

			var path = ResolveAssemblyPath(asm.Name);

			if (path == null)
				return null;

			if (Plugins.Items is not null && Plugins.ShadowCopy)
			{
				var shadowCopyLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Plugins");

				if (!_cleaned)
				{
					_cleaned = true;

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

         Console.WriteLine($"Loading {asm.FullName}");
         return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
		}

		[DebuggerStepThrough]
		public static T GetService<T>()
		{
			return _sm.Get<T>();
		}

		public static void RegisterService(Type contract, object instance)
		{
			_sm.Register(contract, instance);

			ServiceRegistered?.Invoke(contract, EventArgs.Empty);
		}

		public static void RegisterService(Type contract, ServiceActivatorCallback callback)
		{
			_sm.Register(contract, callback);

			ServiceRegistered?.Invoke(contract, EventArgs.Empty);
		}

		public static JsonDocument Configuration
		{
			get
			{
				if (_configuration is not null)
					return _configuration;

				lock (_sm)
				{
					if (_configuration is not null)
						return _configuration;

					var appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
					var sysPath = Path.Combine(appPath, "sys.json");

					while (!string.IsNullOrWhiteSpace(appPath))
					{
						if (File.Exists(sysPath))
						{
							_configuration = JsonDocument.Parse(File.ReadAllText(sysPath), new JsonDocumentOptions
							{
								AllowTrailingCommas = true
							});

							break;
						}

						if (!sysPath.Contains('\\'))
							throw new NullReferenceException("Could not find sys.json configuration file.");

						appPath = appPath[..appPath.LastIndexOf('\\')];
						sysPath = Path.Combine(appPath, "sys.json");
					}
				}

				return _configuration;
			}
		}

		public static void Configure(IApplicationBuilder app)
		{
			_accessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
		}
	}
}
