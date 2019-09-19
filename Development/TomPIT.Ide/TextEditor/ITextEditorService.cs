using System;
using TomPIT.Middleware;

namespace TomPIT.Ide.TextEditor
{
	public interface ITextEditorService
	{
		ITextEditor GetEditor(IMiddlewareContext context, string language);

		void RegisterEditor(string language, Type editorType);
	}
}
