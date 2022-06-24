using System;

namespace TomPIT.UI.Theming.Parser.Tree
{
	using Importers;
	using Infrastructure;
	using Infrastructure.Nodes;
	using TomPIT.UI.Theming.Exceptions;
	using TomPIT.UI.Theming.Plugins;
	using TomPIT.UI.Theming.Utils;

	public class Import : Directive
	{
		private readonly ReferenceVisitor referenceVisitor = new ReferenceVisitor(true);
		/// <summary>
		///  The original path node
		/// </summary>
		protected Node OriginalPath { get; set; }

		public string Path { get; set; }

		/// <summary>
		///  The inner root - if the action is ImportLess
		/// </summary>
		public Ruleset InnerRoot { get; set; }

		/// <summary>
		///  The inner content - if the action is ImportCss
		/// </summary>
		public string InnerContent { get; set; }

		/// <summary>
		///  The media features (if present)
		/// </summary>
		public Node Features { get; set; }

		/// <summary>
		///  The type of import: reference, inline, less, css, once, multiple or optional
		/// </summary>
		public ImportOptions ImportOptions { get; set; }

		/// <summary>
		/// The action to perform with this node
		/// </summary>
		private ImportAction? _importAction;

		public Import(Quoted path, Value features, ImportOptions option)
			 : this((Node)path, features, option)
		{
			OriginalPath = path;
		}

		public Import(Url path, Value features, ImportOptions option)
			 : this((Node)path, features, option)
		{
			OriginalPath = path;
			Path = path.GetUnadjustedUrl();
		}

		/// <summary>
		///  Create a evaluated node that will render a @import
		/// </summary>
		/// <param name="originalPath"></param>
		/// <param name="features"></param>
		private Import(Node originalPath, Node features)
		{
			OriginalPath = originalPath;
			Features = features;
			_importAction = ImportAction.LeaveImport;
		}

		private Import(Node path, Value features, ImportOptions option)
		{
			if (path == null)
				throw new ParserException("Imports do not allow expressions");

			OriginalPath = path;
			Features = features;
			ImportOptions = option;
		}

		private ImportAction GetImportAction(IImporter importer)
		{
			if (!_importAction.HasValue)
			{
				_importAction = importer.Import(this);
			}

			return _importAction.Value;
		}

		public override void AppendCSS(Env env, Context context)
		{
			ImportAction action = GetImportAction(env.Parser.Importer);
			if (action == ImportAction.ImportNothing)
			{
				return;
			}

			if (action == ImportAction.ImportCss)
			{
				env.Output.Append(InnerContent);
				return;
			}

			env.Output.Append("@import ")
				 .Append(OriginalPath.ToCSS(env));

			if (Features)
			{
				env.Output
					 .Append(" ")
					 .Append(Features);
			}
			env.Output.Append(";");

			if (!env.Compress)
			{
				env.Output.Append("\n");
			}
		}

		public override void Accept(Plugins.IVisitor visitor)
		{
			Features = VisitAndReplace(Features, visitor, true);

			if (_importAction == ImportAction.ImportLess)
			{
				InnerRoot = VisitAndReplace(InnerRoot, visitor);
			}
		}

		public override Node Evaluate(Env env)
		{
			OriginalPath = OriginalPath.Evaluate(env);
			var quoted = OriginalPath as Quoted;
			if (quoted != null)
			{
				Path = quoted.Value;
			}

			ImportAction action = GetImportAction(env.Parser.Importer);
			if (action == Importers.ImportAction.ImportNothing)
			{
				return new NodeList().ReducedFrom<NodeList>(this);
			}

			Node features = null;

			if (Features)
				features = Features.Evaluate(env);

			if (action == ImportAction.LeaveImport)
				return new Import(OriginalPath, features);

			if (action == ImportAction.ImportCss)
			{
				var importCss = new Import(OriginalPath, null) { _importAction = ImportAction.ImportCss, InnerContent = InnerContent };
				if (features)
					return new Media(features, new NodeList() { importCss });
				return importCss;
			}

			using (env.Parser.Importer.BeginScope(this))
			{
				if (IsReference || IsOptionSet(ImportOptions, ImportOptions.Reference))
				{
					// Walk the parse tree and mark all nodes as references.
					IsReference = true;

					Accept(referenceVisitor);
				}
				NodeHelper.RecursiveExpandNodes<Import>(env, InnerRoot);
			}

			var rulesList = new NodeList(InnerRoot.Rules).ReducedFrom<NodeList>(this);
			if (features)
			{
				return new Media(features, rulesList);
			}

			return rulesList;
		}
		private bool IsOptionSet(ImportOptions options, ImportOptions test)
		{
			return (options & test) == test;
		}
	}

	public class ReferenceVisitor : IVisitor
	{
		private readonly bool isReference;

		public ReferenceVisitor(bool isReference)
		{
			this.isReference = isReference;
		}

		public Node Visit(Node node)
		{
			var ruleset = node as Ruleset;
			if (ruleset != null)
			{
				if (ruleset.Selectors != null)
				{
					ruleset.Selectors.Accept(this);
					ruleset.Selectors.IsReference = isReference;
				}

				if (ruleset.Rules != null)
				{
					ruleset.Rules.Accept(this);
					ruleset.Rules.IsReference = isReference;
				}
			}

			var media = node as Media;
			if (media != null)
			{
				media.Ruleset.Accept(this);
				media.Ruleset.IsReference = isReference;
			}

			var nodeList = node as NodeList;
			if (nodeList != null)
			{
				nodeList.Accept(this);
			}
			node.IsReference = isReference;

			return node;
		}
	}

	[Flags]
	public enum ImportOptions
	{
		Once = 1,
		Multiple = 2,
		Optional = 4,
		Css = 8,
		Less = 16,
		Inline = 32,
		Reference = 64
	}
}