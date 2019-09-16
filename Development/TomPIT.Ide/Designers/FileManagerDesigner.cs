using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Ide.Dom.ComponentModel;

namespace TomPIT.Ide.Designers
{
	public abstract class FileManagerDesigner : DomDesigner<ReflectionElement>
	{
		protected FileManagerDesigner(ReflectionElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/FileManager.cshtml";
		public override object ViewModel => this;

		public IMediaResourcesConfiguration Media => Element.Component as IMediaResourcesConfiguration;

		public string MediaName => Media.ComponentName();
	}
}
