using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Compilation
{
	internal class ScriptContext : TenantObject, IScriptContext
	{
		private Dictionary<string, ImmutableArray<PortableExecutableReference>> _references = null;
		private ScriptResolver _sourceResolver = null;
		private AssemblyResolver _assemblyResolver = null;
		private Dictionary<string, IText> _sources = null;
		private StringBuilder _sourceText = null;
		public ScriptContext(ITenant tenant, IText sourceCode) : base(tenant)
		{
			MicroService = Tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());
			LoadScript(sourceCode);
		}

		private IMicroService MicroService { get; }

		private StringBuilder SourceText
		{
			get
			{
				if (_sourceText == null)
					_sourceText = new StringBuilder();

				return _sourceText;
			}
		}
		private void LoadScript(IText sourceCode)
		{
			ProcessScript(Tenant.GetService<IComponentService>().SelectText(MicroService.Token, sourceCode), string.Empty);
		}

		private void ProcessScript(string sourceCode, string basePath)
		{
			if (string.IsNullOrWhiteSpace(sourceCode))
				return;

			string currentLine = null;

			using (var reader = new StringReader(sourceCode))
			{
				while ((currentLine = reader.ReadLine()) != null)
				{
					if (!currentLine.Trim().StartsWith('#'))
						SourceText.AppendLine(currentLine);
					else
					{
						var line = currentLine.Trim();
						var tokens = line.Split(" ".ToCharArray(), 2);

						if (tokens.Length < 2 || tokens[1].Length < 3)
							continue;

						var token = tokens[1].Substring(1, tokens[1].Length - 2);

						if (line.StartsWith("#load"))
						{
							var resolvedReference = ScriptResolver.ResolveReference(token, basePath);

							if (!SourceFiles.ContainsKey(resolvedReference))
							{
								var sourceFile = ScriptResolver.LoadScript(resolvedReference);

								if (sourceFile == null)
									continue;

								SourceFiles.Add(resolvedReference, sourceFile);
								ProcessScript(ScriptResolver.ReadText(resolvedReference)?.ToString(), resolvedReference);
							}
						}
						else if (line.StartsWith("#r"))
						{
							var path = AssemblyResolver.ResolvePath(token, basePath);

							if (References.ContainsKey(path))
								continue;

							References.Add(path, AssemblyResolver.ResolveReference(token, basePath, MetadataReferenceProperties.Assembly));
						}
					}
				}
			}
		}

		public string SourceCode => SourceText.ToString();

		public Dictionary<string, IText> SourceFiles
		{
			get
			{
				if (_sources == null)
					_sources = new Dictionary<string, IText>();

				return _sources;
			}
		}

		public Dictionary<string, ImmutableArray<PortableExecutableReference>> References
		{
			get
			{
				if (_references == null)
					_references = new Dictionary<string, ImmutableArray<PortableExecutableReference>>(StringComparer.InvariantCultureIgnoreCase);

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
					_assemblyResolver = new AssemblyResolver(Tenant, MicroService.Token);

				return _assemblyResolver;
			}
		}
	}
}
