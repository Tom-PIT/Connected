using TomPIT.Design.Ide.Designers;

namespace TomPIT.Ide.Designers.ActionResults
{
	public class ViewResult : Result, IDesignerActionResultView
	{
		public ViewResult(object model, string view) : base(model)
		{
			View = view;
		}

		public string View { get; private set; }
	}
}
