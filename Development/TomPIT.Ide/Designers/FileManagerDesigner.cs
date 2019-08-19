using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Designers;
using TomPIT.Dom;

namespace TomPIT.Designers
{
	public abstract class FileManagerDesigner : DomDesigner<ReflectionElement>
	{
		protected FileManagerDesigner(ReflectionElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/FileManager.cshtml";
		public override object ViewModel => this;

		public IMediaResources Media => Element.Component as IMediaResources;

		public string MediaName => Media.ComponentName(Connection);
	}
}
