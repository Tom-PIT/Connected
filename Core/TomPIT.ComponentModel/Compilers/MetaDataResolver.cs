using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Net;
using TomPIT.Runtime;
using TomPIT.Storage;

namespace TomPIT.Compilers
{
	public class MetaDataResolver : MetadataReferenceResolver
	{
		public MetaDataResolver(ISysContext context, Guid microService)
		{
			Context = context;
			MicroService = microService;
		}

		private ISysContext Context { get; }
		private Guid MicroService { get; }

		public override bool ResolveMissingAssemblies => true;

		protected bool Equals(MetaDataResolver other)
		{
			return Equals(Context, other.Context);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((MetaDataResolver)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 37;

				hashCode = (hashCode * 397) ^ (Context?.GetHashCode() ?? 0);

				return hashCode;
			}
		}

		public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
		{
			var component = Context.GetService<IComponentService>().SelectComponent(MicroService, "Assembly", reference);

			if (component == null)
				return ImmutableArray<PortableExecutableReference>.Empty;

			if (!(Context.GetService<IComponentService>().SelectConfiguration(component.Token) is IAssemblyResource config))
				return ImmutableArray<PortableExecutableReference>.Empty;

			if (config is IAssemblyUploadResource)
				return ResolveUploadReference(config as IAssemblyUploadResource);
			else
				return ResolveFileReference(config as IAssemblyFileSystemResource);
		}

		private ImmutableArray<PortableExecutableReference> ResolveUploadReference(IAssemblyUploadResource d)
		{
			if (d.Blob == Guid.Empty)
				return ImmutableArray<PortableExecutableReference>.Empty;

			var content = Context.GetService<IStorageService>().Download(d.Blob);

			if (content == null || content.Content == null || content.Content.Length == 0)
				return ImmutableArray<PortableExecutableReference>.Empty;

			var mr = MetadataReference.CreateFromImage(ImmutableArray.Create(content.Content));

			return ImmutableArray.Create(mr);
		}

		private ImmutableArray<PortableExecutableReference> ResolveFileReference(IAssemblyFileSystemResource d)
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

		public static Assembly LoadDependency(ISysContext context, Guid microService, string name)
		{
			var component = context.GetService<IComponentService>().SelectComponent(microService, "Assembly", name);

			if (component == null)
				return null;

			if (!(context.GetService<IComponentService>().SelectConfiguration(component.Token) is IAssemblyResource config))
				return null;

			if (config is IAssemblyUploadResource d)
			{
				var content = context.GetService<IStorageService>().Download(d.Blob);

				if (content == null || content.Content == null || content.Content.Length == 0)
					return null;

				return Assembly.Load(content.Content);
			}
			else
				return null;
		}
	}
}
