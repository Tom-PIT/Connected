using System;
using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.IoT
{
	public class IoTTemplate : MicroServiceTemplate
	{
		public override Guid Token { get { return new Guid("{CAB78536-8F33-4782-A755-1D439623A1FB}"); } }
		public override string Name { get { return "IoT"; } }

		public override List<IItemDescriptor> QueryDescriptors(IDomElement parent, string category)
		{
			var r = new List<IItemDescriptor>();

			if (string.Compare(category, "IoTHub", true) == 0)
				r.Add(new ItemDescriptor("IoT Hub", "Hub", typeof(Hub)));

			return r;
		}

		public override List<IDomElement> QueryDomRoot(IEnvironment environment, IDomElement element, Guid microService)
		{
			return new List<IDomElement>
			{
				new CategoryElement(environment, element, "IoTHub", "IoTHubs", "Hubs", "fal fa-folder"),
				new ComponentElement(environment, element, CreateReferences(environment, microService)),
			};
		}
	}
}
