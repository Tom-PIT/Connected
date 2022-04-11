using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel.UI.Theming;

namespace TomPIT.UI.Theming
{
	internal class ThemeFileSet
	{
		private Dictionary<string, List<IThemeFile>> _files;

		public void Add(string fileName, IThemeFile file)
		{
			if (!Files.ContainsKey(fileName))
				Files.Add(fileName, new List<IThemeFile>());

			Files[fileName].Add(file);
		}

		public ImmutableArray<string> GetFileNames()
		{
			return Files.Keys.ToImmutableArray();
		}

		public ImmutableArray<IThemeFile> GetFiles(string fileName)
		{
			return Files[fileName].ToImmutableArray();
		}

		private Dictionary<string, List<IThemeFile>> Files => _files ??= new Dictionary<string, List<IThemeFile>>(StringComparer.OrdinalIgnoreCase);
	}
}
