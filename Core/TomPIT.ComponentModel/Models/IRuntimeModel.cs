using TomPIT.ComponentModel.UI;
using TomPIT.Services;

namespace TomPIT.Models
{
	public interface IRuntimeModel : IExecutionContext, IUIModel, IRequestContextProvider, IComponentModel
	{
		IView ViewConfiguration { get; }
	}
}
