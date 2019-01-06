using System;
using System.Collections.Generic;

namespace TomPIT.Design.Services
{
	internal class CodeCompletionService : ICodeCompletionService
	{
		private Dictionary<string, Type> _providers = null;

		public CodeCompletionService()
		{
			Providers.Add("csharp", typeof(CSharpProvider));
		}

		public ICodeCompletionProvider GetProvider(string language)
		{
			if (Providers.ContainsKey(language))
			{
				var p = Providers[language];

				return p.CreateInstance<ICodeCompletionProvider>();
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
