using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Cdn
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class Subscription : TextConfiguration, ISubscriptionConfiguration
	{
		private ListItems<ISubscriptionEvent> _events = null;

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }

		[Items(DesignUtils.SubscriptionEventsItems)]
		public ListItems<ISubscriptionEvent> Events
		{
			get
			{
				if (_events == null)
					_events = new ListItems<ISubscriptionEvent> { Parent = this };

				return _events;
			}
		}

		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
