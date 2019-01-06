using System;

namespace TomPIT.ComponentModel.Views
{
	public interface IView : IConfiguration, IGraphicInterface
	{
		string Url { get; }
		string Layout { get; }

		ListItems<IViewHelper> Helpers { get; }
	}
}
