using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.ComponentModel.UI.Theming;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.UI.Theming.Importers;
using TomPIT.UI.Theming.Parser;
using TomPIT.UI.Theming.Parser.Tree;

namespace TomPIT.UI.Theming
{
	internal class ThemeFileImporter : TenantObject, IImporter
	{
		private List<string> _imports;
		private readonly List<string> _paths = new List<string>();
		public ThemeFileImporter(ITenant tenant) : base(tenant)
		{

		}

		private List<string> Imports => _imports ??= new List<string>();
		public Func<LessParser> Parser { get; set; }
		public string CurrentDirectory { get; set; }

		public string AlterUrl(string url, List<string> pathList)
		{
			throw new NotImplementedException();
		}

		public IDisposable BeginScope(Import parent)
		{
			return new ImportScope(this, Path.GetDirectoryName(parent.Path));
		}

		public List<string> GetCurrentPathsClone()
		{
			return new List<string>(_paths);
		}

		public IEnumerable<string> GetImports()
		{
			return Imports.ToImmutableArray();
		}

		public ImportAction Import(Import import)
		{
			/*
			 * TODO: implement support for http and file system resource.
			 */
			using var ctx = MicroServiceContext.FromIdentifier(import.Path, Tenant);
			var theme = ComponentDescriptor.Theme(ctx, import.Path);

			try
			{
				theme.Validate();
				theme.ValidateConfiguration();

				IThemeFile file = null;

				foreach(var stylesheet in theme.Configuration.Stylesheets)
				{
					if (stylesheet is IText text && string.Compare(text.FileName, theme.Element, true) == 0)
					{
						file = text as IThemeFile;
						break;
					}
				}

				if (file is null)
					return ImportAction.ImportNothing;

				if (file is ICssFile)
					return ImportAction.ImportCss;
				else if (file is ILessFile || file is ILessIncludeFile)
					return ImportAction.ImportLess;
				else if (file is IStaticResource)
					return ImportAction.ImportLess;
				else
					return ImportAction.ImportNothing;
			}
			catch
			{
				return ImportAction.ImportNothing;
			}
		}

		public void ResetImports()
		{
			Imports.Clear();
		}

		private class ImportScope : IDisposable
		{
			private readonly ThemeFileImporter _importer;

			public ImportScope(ThemeFileImporter importer, string path)
			{
				_importer = importer;
				_importer._paths.Add(path);
			}

			public void Dispose()
			{
				_importer._paths.RemoveAt(_importer._paths.Count - 1);
			}
		}
	}
}
