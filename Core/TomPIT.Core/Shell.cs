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

		static Shell()
		{
			_sm = new ServiceContainer(null);
			AssemblyLoadContext.Default.Resolving += OnResolvingAssembly;
		}

		private static Assembly OnResolvingAssembly(AssemblyLoadContext ctx, AssemblyName asm)
		{
			var appPath = Assembly.GetEntryAssembly().Location;
			var target = Path.Combine(Path.GetDirectoryName(appPath), string.Format("{0}.dll", asm.Name));

			if (File.Exists(target))
				return AssemblyLoadContext.Default.LoadFromAssemblyPath(target);

			if (string.IsNullOrWhiteSpace(Sys.Plugins))
				return null;

			var dirs = Directory.GetDirectories(Sys.Plugins);

			foreach (var i in dirs)
			{
				var files = Directory.GetFiles(i);

				foreach (var j in files)
				{
					var name = Path.GetFileNameWithoutExtension(j);

					if (string.Compare(name, asm.Name, true) == 0)
						return AssemblyLoadContext.Default.LoadFromAssemblyPath(j);
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
	}
}
