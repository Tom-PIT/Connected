using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Exceptions;

namespace TomPIT.Compilation
{
	internal class ScriptContext : TenantObject, IScriptContext
	{
		private Dictionary<string, ImmutableArray<PortableExecutableReference>> _references = null;
		private ScriptResolver _sourceResolver = null;
		private AssemblyResolver _assemblyResolver = null;
		private ConcurrentDictionary<string, IText> _sources = null;
		private StringBuilder _sourceText = null;
		private static Lazy<ConcurrentDictionary<Guid, ScriptContextDescriptor>> _contexts = new Lazy<ConcurrentDictionary<Guid, ScriptContextDescriptor>>();
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
			ProcessScript(sourceCode.Id, Tenant.GetService<IComponentService>().SelectText(MicroService.Token, sourceCode), sourceCode.ResolvePath(Tenant));
		}

		private void ProcessScript(Guid id, string sourceCode, string basePath)
		{
			if (string.IsNullOrWhiteSpace(sourceCode))
				return;

			string currentLine = null;
			var references = new List<string>();

			if (Contexts.TryGetValue(id, out ScriptContextDescriptor existing))
				references = existing.References;
			else
			{
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
								references.Add(token);
							else if (line.StartsWith("#r "))
							{
								var path = AssemblyResolver.ResolvePath(token, basePath);

								if (References.ContainsKey(path))
									continue;

								References.Add(path, AssemblyResolver.ResolveReference(token, basePath, MetadataReferenceProperties.Assembly));
							}
						}
					}
				}

				var descriptor = new ScriptContextDescriptor();

				if (references.Count > 0)
					descriptor.References.AddRange(references);

				if (!Contexts.TryAdd(id, descriptor))
					Contexts.TryGetValue(id, out descriptor);
			}

			Parallel.ForEach(references, (i) =>
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
					sourceFile = ScriptResolver.LoadScript(resolvedReference);
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

		public string SourceCode => SourceText.ToString();

		public ConcurrentDictionary<string, IText> SourceFiles
		{
			get
			{
				if (_sources == null)
					_sources = new ConcurrentDictionary<string, IText>();

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

		public static void RemoveContext(Guid script)
		{
			Contexts.TryRemove(script, out ScriptContextDescriptor _);
		}
		private static ConcurrentDictionary<Guid, ScriptContextDescriptor> Contexts => _contexts.Value;
	}
}
