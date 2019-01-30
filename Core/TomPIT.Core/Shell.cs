using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using TomPIT.Services;

namespace TomPIT
{
	public static class Shell
	{
		private static ServiceContainer _sm = null;
		private static ISys _sys = null;
		private static Type _sysType = null;
		private static IHttpContextAccessor _accessor = null;
		private static Version _version = null;

		static Shell()
		{
			_sm = new ServiceContainer(null);
			AssemblyLoadContext.Default.Resolving += OnResolvingAssembly;
		}

		public static Version Version
		{
			get
			{
				if (_version == null)
					_version = typeof(Shell).Assembly.GetName().Version;

				return _version;
			}
		}

		public static HttpContext HttpContext { get { return _accessor?.HttpContext; } }

		private static Assembly OnResolvingAssembly(AssemblyLoadContext ctx, AssemblyName asm)
		{
			var appPath = Assembly.GetEntryAssembly().Location;
			var target = Path.Combine(Path.GetDirectoryName(appPath), string.Format("{0}.dll", asm.Name));

			if (!string.IsNullOrWhiteSpace(Sys.Plugins))
			{

				var dirs = Directory.GetDirectories(Sys.Plugins);

				foreach (var i in dirs)
				{
					var files = Directory.GetFiles(i);

					foreach (var j in files)
					{
						var name = Path.GetFileNameWithoutExtension(j);

						if (string.Compare(name, asm.Name, true) == 0)
						{
							using (var s = new MemoryStream(File.ReadAllBytes(j)))
							{
								return AssemblyLoadContext.Default.LoadFromStream(s);
							}
						}
					}
				}
			}

			if (File.Exists(target))
			{
				using (var s = new MemoryStream(File.ReadAllBytes(target)))
				{
					return AssemblyLoadContext.Default.LoadFromStream(s);
				}
			}

			return null;
		}

		[DebuggerStepThrough]
		public static T GetService<T>()
		{
			return _sm.Get<T>();
		}

		public static void RegisterService(Type contract, object instance)
		{
			_sm.Register(contract, instance);
		}

		public static void RegisterService(Type contract, ServiceActivatorCallback callback)
		{
			_sm.Register(contract, callback);
		}

		public static void RegisterConfigurationType(Type type)
		{
			_sysType = type;
		}

		public static T GetConfiguration<T>() where T : ISys
		{
			return (T)Sys;
		}

		private static ISys Sys
		{
			get
			{
				if (_sys != null)
					return _sys;

				lock (_sm)
				{
					if (_sys != null)
						return _sys;

					var appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
					var sys = Path.Combine(appPath, "sys.json");

					while (!string.IsNullOrWhiteSpace(appPath))
					{
						if (File.Exists(sys))
						{
							_sys = JsonConvert.DeserializeObject(File.ReadAllText(sys), _sysType) as ISys;
							break;
						}

						if (!sys.Contains('\\'))
						{
							_sys = _sysType.CreateInstance() as ISys;
							break;
						}

						appPath = appPath.Substring(0, appPath.LastIndexOf('\\'));
						sys = Path.Combine(appPath, "sys.json");
					}

				}

				return _sys;
			}
		}

		public static void Configure(IApplicationBuilder app)
		{
			_accessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
		}
	}
}
