using System;
using System.Collections.Concurrent;
using TomPIT.Connectivity;
using TomPIT.Ide.TextServices.CSharp;
using TomPIT.Ide.TextServices.Razor;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Ide.TextServices
{
	internal class TextService : TenantObject, ITextService
	{
		private Lazy<ConcurrentDictionary<string, Type>> _editors = new Lazy<ConcurrentDictionary<string, Type>>();

		public TextService(ITenant tenant) : base(tenant)
		{
			RegisterEditor("csharp", typeof(CSharpEditor));
			RegisterEditor("razor", typeof(RazorEditor));
		}

		public ITextEditor GetEditor(IMicroServiceContext context, string language)
		{
			if (Editors.ContainsKey(language.ToLowerInvariant()))
			{
				var editor = Editors[language.ToLowerInvariant()].CreateInstance<ITextEditor>(new object[] { context });

				if (editor == null)
					return null;

				return editor;
			}

			return null;
		}

		public void RegisterEditor(string language, Type editorType)
		{
			Editors.TryAdd(language.ToLowerInvariant(), editorType);
		}

		private ConcurrentDictionary<string, Type> Editors => _editors.Value;
	}
}
