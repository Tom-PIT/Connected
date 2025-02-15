﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Design;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.Ide.ComponentModel;
using TomPIT.MicroServices.IoT.UI;

namespace TomPIT.MicroServices.IoT.Design
{
	public class IoTTemplate : MicroServiceTemplate
	{
		public override Guid Token { get { return new Guid("{CAB78536-8F33-4782-A755-1D439623A1FB}"); } }
		public override string Name { get { return "IoT"; } }
		private static ConcurrentDictionary<string, IItemDescriptor> _items = null;

		static IoTTemplate()
		{
			_items = new ConcurrentDictionary<string, IItemDescriptor>(new Dictionary<string, IItemDescriptor>{
				{"IoTView", new ItemDescriptor("IoT View", "View", typeof(IoTView)) { Category ="UI", Glyph="fal fa-browser", Ordinal=245} },
				{ "Hub", new ItemDescriptor("IoT Hub", "IoTHub", typeof(Hub)) { Category ="IoT", Glyph="fal fa-wifi",  Ordinal=710} }
		});
		}

		public override List<IItemDescriptor> ProvideGlobalAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}

		public override List<IItemDescriptor> ProvideAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}

		public override List<string> GetApplicationParts()
		{
			return new List<string>
			{
				"TomPIT.IoT.Design.dll"
			};
		}

		public override TemplateKind Kind => TemplateKind.Plugin;
	}
}
