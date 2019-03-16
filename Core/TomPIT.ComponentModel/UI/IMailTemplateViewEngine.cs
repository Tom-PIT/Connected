using System;

namespace TomPIT.UI
{
	public interface IMailTemplateViewEngine
	{
		void Render(Guid token);
	}
}
