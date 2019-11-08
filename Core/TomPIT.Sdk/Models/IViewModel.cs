using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Models
{
	public interface IViewModel : IRuntimeModel, IUIModel, IComponentModel
	{
		IViewConfiguration ViewConfiguration { get; }
		ITempDataProvider TempData { get; }
	}
}
