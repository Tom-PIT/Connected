using System;
using System.Collections.Generic;
using TomPIT.Reflection;

namespace TomPIT.Ide.Analysis.Analyzers
{
	internal class CodeAnalyzerService : ICodeAnalyzerService
	{
		private Dictionary<string, Type> _providers = null;

		public CodeAnalyzerService()
		{
			Providers.Add("csharp", typeof(CSharpAnalyzer));
			Providers.Add("razor", typeof(RazorAnalyzer));
		}

		public ICodeAnalyzer GetAnalyzer(string language)
		{
			if (Providers.ContainsKey(language))
			{
				var p = Providers[language];

				return p.CreateInstance<ICodeAnalyzer>();
			}

			return null;
		}

		public Dictionary<string, Type> Providers
		{
			get
			{
				if (_providers == null)
					_providers = new Dictionary<string, Type>();

				return _providers;
			}
		}
	}
}
