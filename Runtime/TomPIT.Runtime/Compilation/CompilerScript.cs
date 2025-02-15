﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Storage;

namespace TomPIT.Compilation
{
	internal class CompilerScript : TenantObject, IDisposable
	{
		public CompilerScript(ITenant tenant, Guid microService, IText sourceCode) : base(tenant)
		{
			MicroService = microService;
			SourceCode = sourceCode;
		}
		public Guid MicroService { get; }
		public IText SourceCode { get; }
		public List<Guid> ScriptReferences { get; private set; }
		private IScriptContext ScriptContext { get; set; }
		public void Create()
		{
			string code = Tenant.GetService<IComponentService>().SelectText(MicroService, SourceCode);

			if (string.IsNullOrWhiteSpace(code))
				return;

			var scriptName = ResolveScriptName();
			code = $"#load \"{scriptName}\"{System.Environment.NewLine}global using LengthAttribute = TomPIT.Annotations.Models.LengthAttribute;{System.Environment.NewLine}";

			var msv = Tenant.GetService<IMicroServiceService>().Select(MicroService);

			using var loader = new InteractiveAssemblyLoader();
			ScriptContext = Tenant.GetService<ICompilerService>().CreateScriptContext(SourceCode);

			var refs = new List<Guid>();

			foreach (var reference in ScriptContext.SourceFiles)
				refs.Add(reference.Value.Id);

			if (refs.Count > 0)
				ScriptReferences = refs;

			var sourceFiles = Shell.Configuration.GetRequiredSection("sourceFiles").GetValue<string>("folder");
			var filePath = Path.Combine(sourceFiles, msv.Token.ToString(), $"{SourceCode.TextBlob}-{BlobTypes.SourceText}.txt");

			var options = ScriptOptions.Default
					.WithImports(Usings)
					.WithReferences(References)
					.WithSourceResolver(new ScriptResolver(Tenant, MicroService))
					.WithMetadataResolver(new AssemblyResolver(Tenant, MicroService, true))
					.WithEmitDebugInformation(Tenant.GetService<IRuntimeService>().Stage != EnvironmentStage.Production)
					.WithFilePath(filePath)
					.WithFileEncoding(Encoding.UTF8)
					.WithOptimizationLevel(Tenant.GetService<IRuntimeService>().Stage == EnvironmentStage.Production ? OptimizationLevel.Release : OptimizationLevel.Debug);

			foreach (var reference in ScriptContext.References)
			{
				if (reference.Value == ImmutableArray<PortableExecutableReference>.Empty)
					continue;

				foreach (var executable in reference.Value)
				{
					/*
		* in memory assembly
		*/
					if (string.IsNullOrEmpty(executable.FilePath))
					{
						var tokens = reference.Key.Split('/');
						var ms = MicroService;
						var name = reference.Key;

						if (tokens.Length > 1)
						{
							ms = Tenant.GetService<IMicroServiceService>().Select(tokens[0]).Token;
							name = tokens[1];
						}

						var asm = AssemblyResolver.LoadDependency(Tenant, ms, name);

						if (asm != null)
							loader.RegisterDependency(asm);
					}
					else
					{
						var name = AssemblyLoadContext.GetAssemblyName(executable.FilePath);

						if (name != null)
						{
							var asm = AssemblyLoadContext.Default.LoadFromAssemblyName(name);

							if (asm != null)
								loader.RegisterDependency(asm);
						}
					}
				}
			}

			Script = CreateScript($"{code};{System.Environment.NewLine}{GenerateStaticCode()}", options, loader);
		}

		private bool ResolveRequiresSyntaxRoot()
		{
			var component = Tenant.GetService<IComponentService>().SelectComponent(SourceCode.Configuration().Component);
			/*
			 * There is a bug in the configuration files. IoC containers have PublicScript namespace which is wrong because they
			 * can't be references thus we are handling IoC containers separately.
			 */
			return !string.Equals(component.Category, ComponentCategories.IoCContainer, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(component.NameSpace, ComponentCategories.NameSpacePublicScript, StringComparison.OrdinalIgnoreCase);
		}

		private string ResolveScriptName()
		{
			var element = Tenant.GetService<IDiscoveryService>().Configuration.Find(SourceCode.Configuration().Component, SourceCode.Id) as IText;
			var ms = Tenant.GetService<IMicroServiceService>().Select(element.Configuration().MicroService());

			if (element is ComponentModel.IConfiguration config)
				return $"{ms.Name}/{element.Configuration().ComponentName()}.csx";
			else
				return $"{ms.Name}/{element.Configuration().ComponentName()}/{element.FileName}";
		}

		protected virtual Script<object> CreateScript(string sourceCode, ScriptOptions options, InteractiveAssemblyLoader loader)
		{
			return CSharpScript.Create(sourceCode, options: options, assemblyLoader: loader);
		}

		protected virtual List<Assembly> References
		{
			get
			{
				var result = new List<Assembly>
				{
					CompilerService.LoadSystemAssembly("TomPIT.Core"),
					CompilerService.LoadSystemAssembly("TomPIT.ComponentModel"),
					CompilerService.LoadSystemAssembly("TomPIT.Sdk"),
					CompilerService.LoadSystemAssembly("TomPIT.Runtime"),
					CompilerService.LoadSystemAssembly("Newtonsoft.Json")
				};

				var references = LoadMicroServiceReferences(MicroService);

				if (references is not null)
					result.AddRange(references);

				return result;
			}
		}

		public static List<Assembly> LoadMicroServiceReferences(Guid microService)
		{
			var result = new List<Assembly>();
			var references = TomPIT.Tenant.GetService<IDiscoveryService>().MicroServices.References.References(microService, false);

			foreach (var reference in references)
			{
				var asm = CompilerService.LoadSystemAssembly($"{reference.Name}.dll");

				if (asm is not null)
					result.Add(asm);
			}

			var ms = TomPIT.Tenant.GetService<IMicroServiceService>().Select(microService);
			var self = CompilerService.LoadSystemAssembly($"{ms.Name}.dll");

			if (self is not null)
				result.Add(self);

			return result;
		}
		protected virtual string[] Usings => new string[] { };

		public void Dispose()
		{
			Script = null;
		}

		public Script<object> Script { get; private set; }

		private string GenerateStaticCode()
		{
			var sb = new StringBuilder();

			sb.AppendLine($"public static class {CompilerService.ScriptInfoClassName}");
			sb.AppendLine("{");
			sb.AppendLine("private static readonly System.Collections.Generic.List<TomPIT.Compilation.SourceTypeDescriptor> _sourceTypes = new System.Collections.Generic.List<TomPIT.Compilation.SourceTypeDescriptor>{");

			foreach (var file in ScriptContext.SourceFiles)
			{
				var config = file.Value.Configuration();
				var component = Tenant.GetService<IComponentService>().SelectComponent(config.Component);
				var declaredTypes = SyntaxBrowser.QueryDeclaredTypes(DocumentIdentity.From(config.MicroService(), component.Token, file.Value.Id));

				foreach (var declaredType in declaredTypes)
				{
					var resolved = ResolveTypeDeclaration(declaredType);

					if (string.IsNullOrEmpty(resolved.Item1))
						continue;

					sb.AppendLine("new TomPIT.Compilation.SourceTypeDescriptor{");
					sb.AppendLine($"Component = new System.Guid(\"{component.Token}\"),");
					sb.AppendLine($"ContainingType = \"{resolved.Item2}\",");
					sb.AppendLine($"TypeName = \"{resolved.Item1}\",");
					sb.AppendLine($"Script = new System.Guid(\"{file.Value.TextBlob}\")");
					sb.AppendLine("},");
				}
			}

			sb.AppendLine("};");
			sb.AppendLine($"public static System.Guid MicroService => new System.Guid(\"{MicroService}\");");
			sb.AppendLine($"public static System.Guid SourceCode => new System.Guid(\"{SourceCode.TextBlob}\");");
			sb.AppendLine($"public static System.Guid Component => new System.Guid(\"{SourceCode.Configuration().Component}\");");
			sb.AppendLine($"public static System.Collections.Generic.List<TomPIT.Compilation.SourceTypeDescriptor> SourceTypes => _sourceTypes;");
			sb.AppendLine("}");

			return sb.ToString();
		}

		private static (string, string) ResolveTypeDeclaration(TypeDeclarationSyntax syntax)
		{
			if (syntax.Modifiers.Any(f => f.IsKind(SyntaxKind.PartialKeyword)))
				return (null, null);

			var name = syntax.Identifier.Text;
			var containingTypeChain = new List<string>();

			var parent = syntax.Parent;

			while (parent is not null && parent.IsKind(SyntaxKind.ClassDeclaration))
			{
				var declaration = parent as ClassDeclarationSyntax;

				containingTypeChain.Add(declaration.Identifier.Text);

				parent = parent.Parent;
			}

			if (containingTypeChain.Any())
				containingTypeChain.Reverse();

			return (name, string.Join('.', containingTypeChain));
		}
	}
}