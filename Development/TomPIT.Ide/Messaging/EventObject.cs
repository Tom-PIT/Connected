﻿using System;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Messaging
{
	internal class EventObject : EnvironmentObject, IEvent
	{
		public EventObject(IDomElement element) : base(element.Environment)
		{
			Element = element;
		}

		public string Name { get; set; }
		public Guid Id { get; set; }
		public IDomElement Element { get; }
		public string Glyph { get; set; }
		public Guid Blob { get; set; }
	}
}