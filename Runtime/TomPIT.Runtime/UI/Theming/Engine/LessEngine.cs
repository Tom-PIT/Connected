﻿using System;

namespace TomPIT.UI.Theming.Engine
{
	using System.Collections.Generic;
	using TomPIT.UI.Theming.Configuration;
	using TomPIT.UI.Theming.Exceptions;
	using TomPIT.UI.Theming.Loggers;
	using TomPIT.UI.Theming.Parser;
	using TomPIT.UI.Theming.Parser.Infrastructure;
	using TomPIT.UI.Theming.Parser.Tree;
	using TomPIT.UI.Theming.Plugins;
	using TomPIT.UI.Theming.Stylizers;

	public class LessEngine : ILessEngine
	{
		public Parser.LessParser Parser { get; set; }
		public ILogger Logger { get; set; }
		public bool Compress { get; set; }
		public bool Debug { get; set; }
		[Obsolete("The Variable Redefines feature has been removed to align with less.js")]
		public bool DisableVariableRedefines { get; set; }
		[Obsolete("The Color Compression feature has been removed to align with less.js")]
		public bool DisableColorCompression { get; set; }
		public bool KeepFirstSpecialComment { get; set; }
		public bool StrictMath { get; set; }
		public Env Env { get; set; }
		public IEnumerable<IPluginConfigurator> Plugins { get; set; }
		public bool LastTransformationSuccessful { get; private set; }

		public string CurrentDirectory
		{
			get { return Parser.CurrentDirectory; }
			set { Parser.CurrentDirectory = value; }
		}

		public LessEngine(Parser.LessParser parser, ILogger logger, LessConfiguration config)
			 : this(parser, logger, config.MinifyOutput, config.Debug, config.DisableVariableRedefines, config.DisableColorCompression, config.KeepFirstSpecialComment, config.Plugins)
		{
		}

		public LessEngine(Parser.LessParser parser, ILogger logger, bool compress, bool debug, bool disableVariableRedefines, bool disableColorCompression, bool keepFirstSpecialComment, bool strictMath, IEnumerable<IPluginConfigurator> plugins)
		{
			Parser = parser;
			Logger = logger;
			Compress = compress;
			Debug = debug;
			Plugins = plugins;
			KeepFirstSpecialComment = keepFirstSpecialComment;
			StrictMath = strictMath;
		}

		public LessEngine(Parser.LessParser parser, ILogger logger, bool compress, bool debug, bool disableVariableRedefines, bool disableColorCompression, bool keepFirstSpecialComment, IEnumerable<IPluginConfigurator> plugins)
			 : this(parser, logger, compress, debug, disableVariableRedefines, disableColorCompression, keepFirstSpecialComment, false, plugins)
		{
		}

		public LessEngine(Parser.LessParser parser, ILogger logger, bool compress, bool debug)
			 : this(parser, logger, compress, debug, false, false, false, null)
		{
		}

		public LessEngine(Parser.LessParser parser, ILogger logger, bool compress, bool debug, bool disableVariableRedefines)
			 : this(parser, logger, compress, debug, disableVariableRedefines, false, false, null)
		{
		}

		public LessEngine(Parser.LessParser parser)
			 : this(parser, new ConsoleLogger(LogLevel.Error), false, false, false, false, false, null)
		{
		}

		public LessEngine()
			 : this(new Parser.LessParser())
		{
		}

		public string TransformToCss(string source, string fileName)
		{
			try
			{
				Parser.StrictMath = StrictMath;
				var tree = Parser.Parse(source, fileName);

				var env = Env ??
							 new Env(Parser)
							 {
								 Compress = Compress,
								 Debug = Debug,
								 KeepFirstSpecialComment = KeepFirstSpecialComment,
							 };

				if (Plugins != null)
				{
					foreach (IPluginConfigurator configurator in Plugins)
					{
						env.AddPlugin(configurator.CreatePlugin());
					}
				}

				var css = tree.ToCSS(env);

				var stylizer = new PlainStylizer();

				foreach (var unmatchedExtension in env.FindUnmatchedExtensions())
				{
					Logger.Warn("Warning: extend '{0}' has no matches {1}\n",
						 unmatchedExtension.BaseSelector.ToCSS(env).Trim(),
						 stylizer.Stylize(new Zone(unmatchedExtension.Extend.Location)).Trim());
				}

				tree.Accept(DelegateVisitor.For<Media>(m =>
				{
					foreach (var unmatchedExtension in m.FindUnmatchedExtensions())
					{
						Logger.Warn("Warning: extend '{0}' has no matches {1}\n",
							 unmatchedExtension.BaseSelector.ToCSS(env).Trim(),
							 stylizer.Stylize(new Zone(unmatchedExtension.Extend.Location)).Trim());
					}
				}));

				LastTransformationSuccessful = true;
				return css;
			}
			catch (ParserException e)
			{
				LastTransformationSuccessful = false;
				LastTransformationError = e;
				Logger.Error(e.Message);
			}

			return "";
		}

		public ParserException LastTransformationError { get; set; }

		public IEnumerable<string> GetImports()
		{
			return Parser.Importer.GetImports();
		}

		public void ResetImports()
		{
			Parser.Importer.ResetImports();
		}

	}
}
