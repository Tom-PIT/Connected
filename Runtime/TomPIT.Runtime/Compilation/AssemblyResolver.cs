using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;
using TomPIT.Runtime;
using TomPIT.Storage;

namespace TomPIT.Compilation
{
	public class AssemblyResolver : MetadataReferenceResolver
	{
		private static ConcurrentDictionary<string, PortableExecutableReference> _metaReferenceCache;

		static AssemblyResolver()
		{
			_metaReferenceCache = new ConcurrentDictionary<string, PortableExecutableReference>();
		}

		public AssemblyResolver(ITenant tenant, Guid microService, bool reflectionOnly)
		{
			Tenant = tenant;
			MicroService = microService;
			ReflectionOnly = reflectionOnly;
		}

		private static ConcurrentDictionary<string, PortableExecutableReference> MetaReferenceCache => _metaReferenceCache;

		private bool ReflectionOnly { get; }
		private ITenant Tenant { get; }
		private Guid MicroService { get; }

		public override bool ResolveMissingAssemblies => true;

		protected bool Equals(AssemblyResolver other)
		{
			return Equals(Tenant, other.Tenant);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			if (obj.GetType() != GetType())
				return false;

			return Equals((AssemblyResolver)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 37;

				hashCode = (hashCode * 397) ^ (Tenant?.GetHashCode() ?? 0);

				return hashCode;
			}
		}

		public ImmutableArray<PortableExecutableReference> Resolve(ITenant tenant, Guid microService, string reference)
		{
			var result = ResolvePackage(tenant, microService, reference);

			if (!result.IsDefaultOrEmpty)
				return result;

			return ResolveAssembly(tenant, microService, reference);
		}

		private ImmutableArray<PortableExecutableReference> ResolvePackage(ITenant tenant, Guid microService, string reference)
		{
			var tokens = reference.Split("/");

			if (tokens.Length != 2)
				return ImmutableArray<PortableExecutableReference>.Empty;

			var ms = tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return ImmutableArray<PortableExecutableReference>.Empty;

			var component = tenant.GetService<IComponentService>().SelectComponentByNameSpace(ms.Token, ComponentCategories.NameSpaceNuGet, tokens[1]);

			if (component == null)
				return ImmutableArray<PortableExecutableReference>.Empty;

			var configuration = tenant.GetService<IComponentService>().SelectConfiguration(component.Token);

			if (configuration is INuGetPackageEmbeddedResource embedded)
			{
				var nuget = ((CompilerService)tenant.GetService<ICompilerService>()).Nuget;
				var asms = nuget.Resolve(embedded.Blob, ReflectionOnly);

				if (asms.IsEmpty)
					return ImmutableArray<PortableExecutableReference>.Empty;

				ImmutableArray<PortableExecutableReference> result = ImmutableArray<PortableExecutableReference>.Empty;

				foreach (var asm in asms)
				{
					try
					{
						result = result.Add(MetadataReference.CreateFromFile(asm.Location));
					}
					catch { }
				}

				return result;
			}
			else if (configuration is INuGetPackageResource resource)
			{
				var nuget = ((CompilerService)tenant.GetService<ICompilerService>()).Nuget;
				var asms = nuget.Resolve(resource.Id, resource.Version, ReflectionOnly);

				if (asms.IsEmpty)
					return ImmutableArray<PortableExecutableReference>.Empty;

				ImmutableArray<PortableExecutableReference> result = ImmutableArray<PortableExecutableReference>.Empty;

				foreach (var asm in asms)
				{
					try
					{
						result = result.Add(MetadataReference.CreateFromFile(asm.Location));
					}
					catch { }
				}

				return result;
			}
			else
				return ImmutableArray<PortableExecutableReference>.Empty;
		}

		private static ImmutableArray<PortableExecutableReference> ResolveAssembly(ITenant tenant, Guid microService, string reference)
		{
			var component = tenant.GetService<IComponentService>().SelectComponent(microService, ComponentCategories.EmbeddedAssembly, reference);

			if (component == null)
			{
				var path = Shell.ResolveAssemblyPath(reference);

				if (string.IsNullOrWhiteSpace(path))
					return ImmutableArray<PortableExecutableReference>.Empty;

				try
				{
					return ImmutableArray.Create(MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(path), reference)));
				}
				catch (FileNotFoundException)
				{
					return ImmutableArray<PortableExecutableReference>.Empty;
				}
			}

			if (!(tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IAssemblyResource config))
				return ImmutableArray<PortableExecutableReference>.Empty;

			if (config is IAssemblyEmbeddedResource)
				return ResolveUploadReference(tenant, config as IAssemblyEmbeddedResource);
			else
				return ResolveFileReference(config as IAssemblyFileSystemResource);
		}

		public static string ResolvePath(string reference, string baseFilePath)
		{
			if (string.IsNullOrWhiteSpace(baseFilePath))
				return reference;

			var tokens = baseFilePath.Split('/');

			if (tokens.Length < 2)
				return reference;

			return $"{tokens[0]}/{reference}";
		}

		public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
		{
			var path = ResolvePath(reference, baseFilePath);
			var tokens = path.Split('/');

			if (tokens.Length < 2)
				return Resolve(Tenant, MicroService, reference);

			var ms = Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			return Resolve(Tenant, ms.Token, reference);
		}

		private static ImmutableArray<PortableExecutableReference> ResolveUploadReference(ITenant tenant, IAssemblyEmbeddedResource d)
		{
			if (d.Blob == Guid.Empty)
				return ImmutableArray<PortableExecutableReference>.Empty;

			var content = tenant.GetService<IStorageService>().Download(d.Blob);

			if (content == null || content.Content == null || content.Content.Length == 0)
				return ImmutableArray<PortableExecutableReference>.Empty;

			var mr = MetadataReference.CreateFromStream(new MemoryStream(content.Content));

			return ImmutableArray.Create(mr);
		}

		private static ImmutableArray<PortableExecutableReference> ResolveFileReference(IAssemblyFileSystemResource d)
		{
			var rs = Shell.GetService<IRuntimeService>();
			var mr = MetadataReference.CreateFromFile(string.Format(@"{0}\bin\{1}", rs.ContentRoot, d.FileName));

			return ImmutableArray.Create(mr);
		}

		public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
		{
			var d = referenceIdentity.GetDisplayName();

			if (MetaReferenceCache.TryGetValue(d, out PortableExecutableReference existing))
				return existing;

			try
			{
				var asm = AppDomain.CurrentDomain.Load(d);

				var r = MetadataReference.CreateFromFile(asm.Location);

				MetaReferenceCache.TryAdd(d, r);

				return r;
			}
			catch
			{
				//return ResolveUploadReference(Tenant, )
				return base.ResolveMissingAssembly(definition, referenceIdentity);
			}
		}

		public static Assembly LoadDependency(ITenant tenant, Guid microService, string name)
		{
			var component = tenant.GetService<IComponentService>().SelectComponent(microService, ComponentCategories.EmbeddedAssembly, name);

			if (component == null)
				return null;

			if (!(tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IAssemblyResource config))
				return null;

			if (config is IAssemblyEmbeddedResource d)
			{
				var content = tenant.GetService<IStorageService>().Download(d.Blob);

				if (content == null || content.Content == null || content.Content.Length == 0)
					return null;

				return Assembly.Load(content.Content);
			}
			else
				return null;
		}
	}
}
