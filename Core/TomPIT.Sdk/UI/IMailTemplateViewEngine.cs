using System;
using System.Threading.Tasks;

namespace TomPIT.UI
{
	public interface IMailTemplateViewEngine
	{
		Task Render(Guid token);
	}
}
