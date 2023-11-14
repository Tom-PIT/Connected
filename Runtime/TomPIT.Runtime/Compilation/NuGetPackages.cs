using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Runtime;
using TomPIT.Storage;

namespace TomPIT.Compilation
{
	internal class NuGetPackages : TenantObject, IDisposable
	{
		private const string FrameworkVersion = "net7.0";
		private const string RepositoryUrl = "https://api.nuget.org/v3/index.json";

		private Lazy<ConcurrentDictionary<string, ManualResetEvent>> _packageLoadState = new Lazy<ConcurrentDictionary<string, ManualResetEvent>>();
		private Lazy<ConcurrentDictionary<Guid, ManualResetEvent>> _packageBlobLoadState = new Lazy<ConcurrentDictionary<Guid, ManualResetEvent>>();
		private Lazy<ConcurrentDictionary<string, bool>> _initializeState = new Lazy<ConcurrentDictionary<string, bool>>();
		private Lazy<ConcurrentDictionary<Guid, bool>> _initializeBlobState = new Lazy<ConcurrentDictionary<Guid, bool>>();
		private Lazy<ConcurrentDictionary<string, List<PackageFileDescriptor>>> _cache = new Lazy<ConcurrentDictionary<string, List<PackageFileDescriptor>>>();
		private Lazy<ConcurrentDictionary<Guid, List<PackageFileDescriptor>>> _blobCache = new Lazy<ConcurrentDictionary<Guid, List<PackageFileDescriptor>>>();
		private SourceCacheContext _cacheContext;
		private ILogger _logger;
		private NuGetFramework _framework;
		private SourceRepository _repository;
		private string _root;
		private PackagePathResolver _pathResolver;
		public NuGetPackages(ITenant tenant) : base(tenant)
		{
			var rt = Tenant.GetService<IRuntimeService>();

			ReadOnly = rt.Connectivity == EnvironmentConnectivity.Offline;

			System.Environment.SetEnvironmentVariable("NUGET_HTTP_CACHE_PATH", "/home/tompit/TomPITPackageCache");
		}

		private bool ReadOnly { get; }
		private ConcurrentDictionary<string, ManualResetEvent> LoadState => _packageLoadState.Value;
		private ConcurrentDictionary<Guid, ManualResetEvent> LoadBlobState => _packageBlobLoadState.Value;
		private ConcurrentDictionary<string, bool> InitializeState => _initializeState.Value;
		private ConcurrentDictionary<Guid, bool> InitializeBlobState => _initializeBlobState.Value;
		private ConcurrentDictionary<string, List<PackageFileDescriptor>> Cache => _cache.Value;
		private ConcurrentDictionary<Guid, List<PackageFileDescriptor>> BlobCache => _blobCache.Value;
		private SourceCacheContext CacheContext => _cacheContext ??= new SourceCacheContext();
		private ILogger Logger => _logger ??= new NuGetLogger(Tenant);
		private NuGetFramework Framework => _framework ??= NuGetFramework.ParseFolder(FrameworkVersion);
		private SourceRepository Repository
		{
			get
			{
				if (_repository == null)
				{
					_repository = NuGet.Protocol.Core.Types.Repository.Factory.GetCoreV3(RepositoryUrl);

					_repository.PackageSource.IsMachineWide = false;
				}

				return _repository;
			}
		}

		private string RootDirectory
		{
			get
			{
				if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
				{
					return _root ??= Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".tompit", "packages");
				}
				else
				{
					return _root ??= "/home/tompit/packages";
				}
			}
		}

		private PackagePathResolver PathResolver => _pathResolver ??= new PackagePathResolver(RootDirectory, false);

		public ImmutableList<Assembly> Resolve(Guid blob, bool entryOnly)
		{
			return Resolve(GetPackageFileSet(blob).Result, entryOnly);
		}
		public ImmutableList<Assembly> Resolve(string id, string version, bool entryOnly)
		{
			return Resolve(GetPackageFileSet(id, version).Result, entryOnly);
		}

		private ImmutableList<Assembly> Resolve(ImmutableArray<PackageFileDescriptor> files, bool entryOnly)
		{
			var result = new List<Assembly>();

			if (files.Any())
			{
				if (entryOnly)
				{
					var file = files.FirstOrDefault(f => f.Entry);

					if (file != null)
					{
						var asm = LoadAssembly(file);

						if (asm != null)
							result.Add(asm);
					}
				}
				else
				{
					foreach (var file in files)
					{
						var asm = LoadAssembly(file);

						if (asm != null)
							result.Add(asm);
					}
				}
			}

			return result.ToImmutableList();
		}

		private async Task<ImmutableArray<PackageFileDescriptor>> GetPackageFileSet(Guid blob)
		{
			if (blob == Guid.Empty)
				return ImmutableArray<PackageFileDescriptor>.Empty;

			if (InitializeBlobState.TryGetValue(blob, out bool initialized) && initialized)
			{
				if (BlobCache.TryGetValue(blob, out List<PackageFileDescriptor> _result))
					return _result.ToImmutableArray();

				return ImmutableArray<PackageFileDescriptor>.Empty;
			}

			var resetEvent = new ManualResetEvent(false);

			if (!LoadBlobState.TryAdd(blob, resetEvent))
			{
				resetEvent.Dispose();
				resetEvent = LoadBlobState[blob];

				resetEvent.WaitOne();

				if (BlobCache.TryGetValue(blob, out List<PackageFileDescriptor> _result))
					return _result.ToImmutableArray();

				return ImmutableArray<PackageFileDescriptor>.Empty;
			}
			else
			{
				try
				{
					var result = await RestorePackage(blob);

					InitializeBlobState.TryAdd(blob, true);

					return result;
				}
				finally
				{
					resetEvent.Set();

					if (LoadBlobState.TryRemove(blob, out ManualResetEvent e))
						e.Dispose();
				}
			}

		}
		private async Task<ImmutableArray<PackageFileDescriptor>> GetPackageFileSet(string id, string version)
		{
			var key = CreateKey(id, version);

			if (InitializeState.TryGetValue(key, out bool initialized) && initialized)
			{
				if (Cache.TryGetValue(key, out List<PackageFileDescriptor> _result))
					return _result.ToImmutableArray();

				return ImmutableArray<PackageFileDescriptor>.Empty;
			}

			var resetEvent = new ManualResetEvent(false);

			if (!LoadState.TryAdd(key, resetEvent))
			{
				resetEvent.Dispose();
				resetEvent = LoadState[key];

				resetEvent.WaitOne();

				if (Cache.TryGetValue(key, out List<PackageFileDescriptor> _result))
					return _result.ToImmutableArray();

				return ImmutableArray<PackageFileDescriptor>.Empty;
			}
			else
			{
				try
				{
					var result = await RestorePackage(id, version);

					InitializeState.TryAdd(key, true);

					return result;
				}
				finally
				{
					resetEvent.Set();

					if (LoadState.TryRemove(key, out ManualResetEvent e))
						e.Dispose();
				}
			}
		}

		private static string CreateKey(string id, string version)
		{
			return $"{id}/{version}".ToLowerInvariant();
		}

		private Assembly LoadAssembly(PackageFileDescriptor descriptor)
		{
			try
			{
				foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
				{
					if (string.IsNullOrEmpty(assembly.FullName))
						continue;

					var name = new AssemblyName(assembly.FullName);

					if (!string.Equals(descriptor.Name, name.Name, StringComparison.Ordinal))
						continue;

					if (Version.TryParse(descriptor.Version, out Version? descriptorVersion) && descriptorVersion > name.Version)
						continue;

					return assembly;
				}

				return AssemblyLoadContext.Default.LoadFromAssemblyPath(descriptor.FileName);
			}
			catch
			{
				return null;
			}
		}

		private async Task<ImmutableArray<PackageFileDescriptor>> RestorePackage(Guid blob)
		{
			var content = Tenant.GetService<IStorageService>().Download(blob);

			if (content == null || content.Content?.Length == 0)
				return ImmutableArray<PackageFileDescriptor>.Empty;

			using var ms = new MemoryStream(content.Content);
			using var reader = new PackageArchiveReader(ms);

			var restoreSet = await CreateRestoreSet(blob, reader);

			var result = await RestorePackage(reader.NuspecReader.GetId(), reader.NuspecReader.GetVersion().ToString(), restoreSet, true);

			BlobCache.TryAdd(blob, result.ToList());

			return result;
		}
		private async Task<ImmutableArray<PackageFileDescriptor>> RestorePackage(string id, string version)
		{
			var restoreSet = await CreateRestoreSet(id, version);

			return await RestorePackage(id, version, restoreSet, false);
		}

		private async Task<ImmutableArray<PackageFileDescriptor>> RestorePackage(string id, string version, IEnumerable<SourcePackageDependencyInfo> packages, bool isBlob)
		{
			var packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.None, null, Logger);
			var frameworkReducer = new FrameworkReducer();
			var files = new List<PackageFileDescriptor>();

			foreach (var installer in packages)
			{
				PackageReaderBase packageReader;
				var installedPath = PathResolver.GetInstalledPath(installer);

				if (string.IsNullOrEmpty(installedPath))
				{
					if (ReadOnly)
						continue;

					var downloadResource = await installer.Source.GetResourceAsync<DownloadResource>(CancellationToken.None);
					var downloadResult = await downloadResource.GetDownloadResourceResultAsync(installer, new PackageDownloadContext(CacheContext), RootDirectory, Logger, CancellationToken.None);

					packageReader = downloadResult.PackageReader;
				}
				else
					packageReader = new PackageFolderReader(installedPath);

				var libItems = packageReader.GetLibItems();
				var nearest = frameworkReducer.GetNearest(Framework, libItems.Select(x => x.TargetFramework));

				if (nearest == null)
					continue;

				var lib = libItems.First(f => string.Compare(f.TargetFramework.DotNetFrameworkName, nearest.DotNetFrameworkName, true) == 0);

				if (lib == null)
					continue;

				/*
			* This is due to the strange behavior when installing packages locally. I couldn't figure out why
			* extractor doesn't install local package in the version folder. It is installed directly in the
			* package's root folder
			*/
				var folder = Path.Combine(PathResolver.GetInstallPath(installer), installer.Version.ToString());

				if (!Directory.Exists(folder))
					folder = PathResolver.GetInstallPath(installer);

				foreach (var item in lib.Items)
				{
					var entry = string.Compare(installer.Id, id, true) == 0;

					if (string.Compare(Path.GetExtension(item), ".dll", true) == 0)
					{
						var fullPath = Path.GetFullPath(Path.Combine(folder, item));

						if (!File.Exists(fullPath))
							continue;

						files.Add(new PackageFileDescriptor
						{
							FileName = fullPath,
							Version = installer.Version.ToString(),
							Name = installer.Id,
							Entry = entry
						});
					}
				}
			}

			if (!isBlob)
				Cache.TryAdd(CreateKey(id, version), files);

			return files.ToImmutableArray();
		}

		private async Task<IEnumerable<SourcePackageDependencyInfo>> CreateRestoreSet(Guid blob, PackageArchiveReader reader)
		{
			var identity = reader.GetIdentity();
			var candidates = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

			await ResolveDependencies(blob, identity, candidates, reader);

			var resolverContext = new PackageResolverContext
			(
				 DependencyBehavior.Lowest, new[] { identity.Id },
				 Enumerable.Empty<string>(),
				 Enumerable.Empty<PackageReference>(),
				 Enumerable.Empty<PackageIdentity>(),
				 candidates,
				 new List<PackageSource> { Repository.PackageSource },
				 Logger
			);
			var resolver = new PackageResolver();

			return resolver.Resolve(resolverContext, CancellationToken.None).Select(p => candidates.Single(x => PackageIdentityComparer.Default.Equals(x, p)));
		}

		private async Task<IEnumerable<SourcePackageDependencyInfo>> CreateRestoreSet(string id, string version)
		{
			var identity = new PackageIdentity(id, NuGetVersion.Parse(version));
			var candidates = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
			var dependencyInfoResource = await Repository.GetResourceAsync<DependencyInfoResource>();
			var dependencyInfo = await dependencyInfoResource.ResolvePackage(identity, Framework, CacheContext, Logger, CancellationToken.None);

			if (dependencyInfo != null)
				await ResolveDependencies(identity, candidates);
			else
				return null;

			var resolverContext = new PackageResolverContext
			(
				 DependencyBehavior.Lowest, new[] { id },
				 Enumerable.Empty<string>(),
				 Enumerable.Empty<PackageReference>(),
				 Enumerable.Empty<PackageIdentity>(),
				 candidates,
				 new List<PackageSource> { Repository.PackageSource },
				 Logger
			);
			var resolver = new PackageResolver();

			return resolver.Resolve(resolverContext, CancellationToken.None).Select(p => candidates.Single(x => PackageIdentityComparer.Default.Equals(x, p)));
		}

		private async Task ResolveDependencies(Guid blob, PackageIdentity package, ISet<SourcePackageDependencyInfo> restoreSet, PackageArchiveReader reader)
		{
			if (restoreSet.Contains(package))
				return;

			var dependencies = new List<PackageDependency>();
			var frameworkReducer = new FrameworkReducer();
			var framework = frameworkReducer.GetNearest(Framework, reader.GetSupportedFrameworks());
			var group = reader.GetPackageDependencies().FirstOrDefault(f => f.TargetFramework == framework);

			if (group != null)
			{
				foreach (var dependency in group.Packages)
					dependencies.Add(dependency);
			}

			var dependencyInfo = new LocalPackageIdentity(reader, blob, Tenant, dependencies);

			if (dependencyInfo == null)
				return;

			restoreSet.Add(dependencyInfo);

			foreach (var dependency in dependencyInfo.Dependencies)
				await ResolveDependencies(new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion), restoreSet);
		}

		private async Task ResolveDependencies(PackageIdentity package, ISet<SourcePackageDependencyInfo> restoreSet)
		{
			if (restoreSet.Contains(package))
				return;

			var dependencyInfoResource = await Repository.GetResourceAsync<DependencyInfoResource>();
			var dependencyInfo = await dependencyInfoResource.ResolvePackage(package, Framework, CacheContext, Logger, CancellationToken.None);

			if (dependencyInfo == null)
				return;

			restoreSet.Add(dependencyInfo);

			foreach (var dependency in dependencyInfo.Dependencies)
				await ResolveDependencies(new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion), restoreSet);
		}

		public void Dispose()
		{
			if (_cacheContext != null)
			{
				_cacheContext.Dispose();
				_cacheContext = null;
			}
		}
	}
}
