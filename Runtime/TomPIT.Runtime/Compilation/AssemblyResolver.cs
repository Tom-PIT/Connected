using System;
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
		public AssemblyResolver(ITenant tenant, Guid microService)
		{
			Tenant = tenant;
			MicroService = microService;
		}

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

		public static ImmutableArray<PortableExecutableReference> Resolve(ITenant tenant, Guid microService, string reference)
		{
			var component = tenant.GetService<IComponentService>().SelectComponent(microService, "Assembly", reference);

			if (component == null)
			{
				var path = Assembly.GetEntryAssembly().Location;

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

			var mr = MetadataReference.CreateFromImage(ImmutableArray.Create(content.Content));

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
			try
			{
				var asm = AppDomain.CurrentDomain.Load(d);

				var r = MetadataReference.CreateFromFile(asm.Location);

				return r;
			}
			catch
			{
				return base.ResolveMissingAssembly(definition, referenceIdentity);
			}
		}

		public static Assembly LoadDependency(ITenant tenant, Guid microService, string name)
		{
			var component = tenant.GetService<IComponentService>().SelectComponent(microService, "Assembly", name);

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
