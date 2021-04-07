using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Reflection.CodeAnalysis;
using TomPIT.Reflection.ManifestProviders;

namespace TomPIT.Reflection
{
	internal class ScriptManifestCompiler : TenantObject, IScriptManifestCompiler
	{
		private ScriptResolver _scriptResolver;
		private List<IScriptManifestProvider> _providers;

		static ScriptManifestCompiler()
		{

		}

		public ScriptManifestCompiler(ITenant tenant, Guid microService, Guid component, Guid element) : base(tenant)
		{
			MicroService = microService;
			Component = component;
			Element = element;
		}

		public List<IScriptManifestProvider> Providers => _providers ??= new List<IScriptManifestProvider>
		{
			new ApiOperationProvider()
		};
		public Guid MicroService { get; }
		public IText Script { get; private set; }
		public Guid Component { get; }
		public Guid Element { get; }
		public SyntaxTree SyntaxTree { get; set; }
		public Microsoft.CodeAnalysis.Compilation Compilation { get; set; }
		public SemanticModel Model { get; set; }
		private ScriptResolver ScriptResolver => _scriptResolver ??= new ScriptResolver(Tenant, MicroService);
		public IScriptManifestProvider Provider { get; private set; }
		public void Compile()
		{
			Manifest = new ScriptManifest(true);

			Manifest.Address = Manifest.GetId(MicroService, Component, Element);

			Prepare();

			if (Model == null)
				return;

			Precompile();
		}

		public IScriptManifest Manifest { get; private set; }

		private void Prepare()
		{
			if (Tenant.GetService<IDiscoveryService>().Configuration.Find(Component, Element) is not IText text)
				return;

			Script = text;

			Compilation = Tenant.GetService<ICompilerService>().GetCompilation(text);

			if (Compilation == null)
				return;

			SyntaxTree = Compilation.SyntaxTrees.FirstOrDefault(f => string.Compare(f.FilePath, text.FileName, true) == 0);

			if (SyntaxTree == null || !SyntaxTree.HasCompilationUnitRoot)
				return;

			Model = Compilation.GetSemanticModel(SyntaxTree);
		}

		private void Precompile()
		{
			PrecompileReferences();
			PrecompileIdentifiers();
		}

		private void PrecompileReferences()
		{
			var root = SyntaxTree.GetCompilationUnitRoot();

			var loadDirectives = root.GetLoadDirectives();

			if (loadDirectives != null)
			{
				foreach (var loadDirective in loadDirectives)
				{
					var reference = CreateScriptReference(loadDirective.File.ValueText);

					if (reference >= 0)
						Manifest.ScriptReferences.Add(reference);
				}
			}

			var refDirectives = root.GetReferenceDirectives();

			if (refDirectives != null)
			{
				foreach (var refDirective in refDirectives)
				{
					var id = CreateResourceReference(refDirective.File.ValueText);

					if (id >= 0)
						Manifest.ResourceReferences.Add(id);
				}
			}
		}

		private void PrecompileIdentifiers()
		{
			var root = SyntaxTree.GetCompilationUnitRoot();
			var resolver = new MemberParser(this);

			foreach (var member in root.DescendantNodesAndSelf())
			{
				if (member is MemberDeclarationSyntax declaration)
					resolver.Resolve(declaration);
			}
		}

		private static CSharpParseOptions CreateParserOptions()
		{
			return new CSharpParseOptions()
				.WithDocumentationMode(DocumentationMode.Diagnose)
				.WithKind(SourceCodeKind.Script);
		}

		private short CreateResourceReference(string fileName)
		{
			var tokens = fileName.Split('/');

			if (tokens.Length == 0)
				return -1;

			var ms = Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return -1;

			var component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(ms.Token, ComponentCategories.NameSpaceNuGet, Path.GetFileNameWithoutExtension(tokens[1]));

			if(component == null)
				component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(ms.Token, ComponentCategories.NameSpaceResource, Path.GetFileNameWithoutExtension(tokens[1]));

			if (component == null)
				return -1;

			return Manifest.GetId(component.MicroService, component.Token, Guid.Empty);
		}

		public IText ResolveScript(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				return null;

			var path = ScriptResolver.ResolveReference(fileName, null);

			if (string.IsNullOrWhiteSpace(path))
				return null;

			return ScriptResolver.LoadScript(path);
		}
		private short CreateScriptReference(string fileName)
		{
			var script = ResolveScript(fileName);

			if (script == null)
				return -1;

			return Manifest.GetId(script.Configuration().MicroService(), script.Configuration().Component, script.Id);
		}
	}
}

