using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	internal class FeaturesCollection : ItemsBase
	{
		public const string Feature = "{4020EF7F-C219-487F-B0E6-77581A0E677B}";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Feature", Feature, typeof(IFeature)));
		}
	}
}
