using System;
using System.ComponentModel;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.MicroServices.UI
{
	public abstract class ViewBase : ComponentConfiguration, IGraphicInterface
	{
		[Browsable(false)]
		public Guid Id => Component;
		[Browsable(false)]
		public IElement Parent => null;
		[Browsable(false)]
		public Guid TextBlob { get; set; }

		public void Reset()
		{

		}
	}
}
