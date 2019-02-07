using TomPIT.ComponentModel.UI;
using TomPIT.Services;

namespace TomPIT.Models
{
	public interface IRuntimeModel : IExecutionContext, IUIModel, IRequestContextProvider, IIdentityBinder, IComponentModel
	{
		IView ViewConfiguration { get; }
	}
}
