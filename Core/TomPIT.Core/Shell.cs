using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Runtime.Configuration;
using TomPIT.Serialization;

namespace TomPIT
{
    public static class Shell
    {
        private static ServiceContainer _sm = null;
        private static ISys _sys = null;
        private static Type _sysType = null;
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

        public static string ResolveAssemblyPath(string assemblyName)
        {
            if (assemblyName.EndsWith(".dll"))
                assemblyName = Path.GetFileNameWithoutExtension(assemblyName);

            var appPath = Assembly.GetEntryAssembly().Location;
            var target = Path.Combine(Path.GetDirectoryName(appPath), string.Format("{0}.dll", assemblyName));

            if (Sys.Plugins != null && !string.IsNullOrWhiteSpace(Sys.Plugins.Location))
            {
                var dirs = Directory.GetDirectories(Sys.Plugins.Location);

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
            var path = ResolveAssemblyPath(asm.Name);

            if (path == null)
                return null;

            if (Sys.Plugins != null && Sys.Plugins.ShadowCopy)
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

                    var bytes = File.ReadAllBytes(path);
                    File.Copy(path, targetPath, true);
                }

                path = targetPath;
            }


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

                    var cb = new ConfigurationBuilder();

                    var appPath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

                    var segments = new Uri(appPath).Segments;

                    segments = segments.Select(e=> e.Replace("%20", " ")).ToArray();

                    for (int i = 1; i <= segments.Length; i++)
                    {
                        var pathBase = Path.Combine(segments[0..i]);
                        var filePath = Path.Combine(pathBase, "sys.json");
                        cb.AddNewtonsoftJsonFile(filePath, true, false);
                    }

                    cb.AddEnvironmentVariables();

                    var config = cb.Build();

                    //Workaround for the JSON interface converter cheat
                    var serializedConfig = JsonConvert.SerializeObject(ToJObject(config));

                    _sys = JsonConvert.DeserializeObject(serializedConfig, _sysType) as ISys;
                }

                return _sys;
            }
        }

        public static void Configure(IApplicationBuilder app)
        {
            _accessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        }

        private static List<string> LoadedAssemblies => _loadedAssemblies.Value;

        private static JToken ToJObject(IConfiguration config)
        {
            var obj = new JObject();

            foreach (var child in config.GetChildren())
            {
                if (child.Path.EndsWith(":0"))
                {
                    var arr = new JArray();

                    foreach (var arrayChild in config.GetChildren())
                    {
                        arr.Add(ToJObject(arrayChild));
                    }

                    return arr;
                }
                else
                {
                    obj.Add(child.Key, ToJObject(child));
                }
            }

            if (!obj.HasValues && config is IConfigurationSection section)
            {
                if (bool.TryParse(section.Value, out bool boolean))
                {
                    return new JValue(boolean);
                }
                else if (decimal.TryParse(section.Value, out decimal real))
                {
                    return new JValue(real);
                }
                else if (long.TryParse(section.Value, out long integer))
                {
                    return new JValue(integer);
                }

                return new JValue(section.Value);
            }

            return obj;
        }
    }
}
