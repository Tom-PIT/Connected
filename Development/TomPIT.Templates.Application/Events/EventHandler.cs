using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Application.Events
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class EventHandler : ComponentConfiguration, IEventHandler
	{
		private ListItems<IEventBinding> _bindings = null;
		public const string ComponentCategory = "EventHandler";

		[Items("TomPIT.Application.Design.Items.EventBindingsCollection, TomPIT.Application.Design")]
		public ListItems<IEventBinding> Events
		{
			get
			{
				if (_bindings == null)
					_bindings = new ListItems<IEventBinding> { Parent = this };

				return _bindings;
			}
		}

		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
