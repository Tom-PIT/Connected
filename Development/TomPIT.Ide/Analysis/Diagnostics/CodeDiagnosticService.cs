using System;
using System.Collections.Generic;
using TomPIT.Reflection;

namespace TomPIT.Ide.Analysis.Diagnostics
{
	internal class CodeDiagnosticService : ICodeDiagnosticService
	{
		private Dictionary<string, Type> _providers = null;

		public CodeDiagnosticService()
		{
			Providers.Add("csharp", typeof(CSharpDiagnosticProvider));
		}

		public ICodeDiagnosticProvider GetProvider(string language)
		{
			if (Providers.ContainsKey(language))
			{
				var p = Providers[language];

				return p.CreateInstance<ICodeDiagnosticProvider>();
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
