using System.Collections.Generic;
using TomPIT.Configuration;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Items
{
	internal class SettingsCollection : ItemsBase
	{
		public const string Setting = "{ADCC4487-6F63-4086-8CB5-26FAC170B452}";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Setting", Setting, typeof(Setting)));
		}
	}
}
