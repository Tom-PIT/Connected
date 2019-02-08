using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Design
{
	public interface IComponentCreateHandler
	{
		void InitializeNewComponent(IExecutionContext context, object instance);
	}
}
