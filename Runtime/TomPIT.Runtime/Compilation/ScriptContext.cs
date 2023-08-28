using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Reflection;

namespace TomPIT.Compilation
{
	internal class ScriptContext : TenantObject, IScriptContext
	{
		private ConcurrentDictionary<string, ImmutableArray<PortableExecutableReference>> _references = null;
		private ScriptResolver _sourceResolver = null;
		private AssemblyResolver _assemblyResolver = null;
		private ConcurrentDictionary<string, IText> _sources = null;
		private static Lazy<ConcurrentDictionary<Guid, ScriptContextDescriptor>> _contexts = new Lazy<ConcurrentDictionary<Guid, ScriptContextDescriptor>>();
		public ScriptContext(ITenant tenant, IText sourceCode) : base(tenant)
		{
			MicroService = Tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());
			LoadScript(sourceCode);
		}

		private IMicroService MicroService { get; }

		private void LoadScript(IText sourceCode)
		{
			ProcessScript(sourceCode.Id, Tenant.GetService<IComponentService>().SelectText(MicroService.Token, sourceCode), sourceCode.ResolvePath(Tenant));
		}

		private void ProcessScript(Guid id, string sourceCode, string basePath)
		{
			if (string.IsNullOrWhiteSpace(sourceCode))
				return;

			var loadReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			if (Contexts.TryGetValue(id, out ScriptContextDescriptor existing))
				loadReferences = existing.LoadReferences;
			else
			{
				var tree = CSharpSyntaxTree.ParseText(sourceCode);

				if (tree == null || !tree.HasCompilationUnitRoot)
					return;

				var root = tree.GetCompilationUnitRoot();
				var loadDirectives = root.GetLoadDirectives();
				var refDirectives = root.GetReferenceDirectives();

				foreach (var load in loadDirectives)
				{
					if (!string.IsNullOrWhiteSpace(load.File.ValueText))
						loadReferences.Add(load.File.ValueText);
				}

				foreach (var reference in refDirectives)
				{
					var file = reference.File.ValueText;

					if (string.IsNullOrWhiteSpace(file))
						continue;
					/*
					 * This is only for backwards compatibillity and will be removed in the future. #r directives should always be defined in format microService/reference.
					 */
					var path = file.Contains("/") ? file : AssemblyResolver.ResolvePath(file, basePath);

					if (References.ContainsKey(path))
						continue;

					References.TryAdd(path, AssemblyResolver.ResolveReference(path, basePath, MetadataReferenceProperties.Assembly));
				}

				var descriptor = new ScriptContextDescriptor();

				foreach (var reference in loadReferences)
					descriptor.LoadReferences.Add(reference);

				if (!Contexts.TryAdd(id, descriptor))
					Contexts.TryGetValue(id, out descriptor);
			}

			Parallel.ForEach(loadReferences, (i) =>
			{
				var resolvedReference = string.Empty;

				try
				{
					resolvedReference = ScriptResolver.ResolveReference(i, basePath);
				}
				catch (RuntimeException ex)
				{
					if (string.IsNullOrWhiteSpace(basePath))
						throw;

					throw new RuntimeException($"{ex.Message} ({basePath})");
				}

				if (SourceFiles.ContainsKey(resolvedReference))
					return;

				IText sourceFile = null;

				try
				{
					sourceFile = Tenant.GetService<IDiscoveryService>().Configuration.Find(resolvedReference);
				}
				catch { }

				if (sourceFile == null)
					return;

				SourceFiles.TryAdd(resolvedReference, sourceFile);

				var text = Tenant.GetService<IComponentService>().SelectText(sourceFile.Configuration().MicroService(), sourceFile);

				if (text != null)
					ProcessScript(sourceFile.Id, text.ToString(), resolvedReference);

			});
		}

		public ConcurrentDictionary<string, IText> SourceFiles
		{
			get
			{
				if (_sources == null)
					_sources = new ConcurrentDictionary<string, IText>();

				return _sources;
			}
		}

		public ConcurrentDictionary<string, ImmutableArray<PortableExecutableReference>> References
		{
			get
			{
				if (_references == null)
					_references = new ConcurrentDictionary<string, ImmutableArray<PortableExecutableReference>>(StringComparer.InvariantCultureIgnoreCase);

				return _references;
			}
		}

		private ScriptResolver ScriptResolver
		{
			get
			{
				if (_sourceResolver == null)
					_sourceResolver = new ScriptResolver(Tenant, MicroService.Token);

				return _sourceResolver;
			}
		}

		private AssemblyResolver AssemblyResolver
		{
			get
			{
				if (_assemblyResolver == null)
					_assemblyResolver = new AssemblyResolver(Tenant, MicroService.Token, false);

				return _assemblyResolver;
			}
		}

		public static void RemoveContext(Guid script)
		{
			Contexts.TryRemove(script, out ScriptContextDescriptor _);
		}
		private static ConcurrentDictionary<Guid, ScriptContextDescriptor> Contexts => _contexts.Value;
	}
}
