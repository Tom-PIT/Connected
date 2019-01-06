using System;
using System.Collections.Generic;
using TomPIT.Application.Apis;
using TomPIT.Application.Data;
using TomPIT.Application.Dom;
using TomPIT.Application.Events;
using TomPIT.Application.Resources;
using TomPIT.Application.UI;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application
{
	public class ApplicationTemplate : MicroServiceTemplate
	{
		public override Guid Token { get { return new Guid("{0FF75674-12F7-4EBB-8CF0-AD08A27319D4}"); } }
		public override string Name { get { return "Business Application"; } }

		public override List<IItemDescriptor> QueryDescriptors(IDomElement parent, string category)
		{
			var r = new List<IItemDescriptor>();

			if (string.Compare(category, Area.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Area", "Area", typeof(Area)));
			else if (string.Compare(category, View.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("View", "View", typeof(View)));
			else if (string.Compare(category, MasterView.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Master view", "MasterView", typeof(MasterView)));
			else if (string.Compare(category, Theme.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Theme", "Theme", typeof(Theme)));
			else if (string.Compare(category, Partial.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Partial view", "PartialView", typeof(Partial)));
			else if (string.Compare(category, Connection.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Data repository", "Repository", typeof(Connection)));
			else if (string.Compare(category, DataSource.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Data source", "DataSource", typeof(DataSource)));
			else if (string.Compare(category, Transaction.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Transaction", "Transaction", typeof(Transaction)));
			else if (string.Compare(category, Api.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Api", "Api", typeof(Api)));
			else if (string.Compare(category, Library.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Library", "Library", typeof(Library)));
			else if (string.Compare(category, Events.EventHandler.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Event handler", Events.EventHandler.ComponentCategory, typeof(Events.EventHandler)));
			else if (string.Compare(category, DataManagement.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Data management", DataManagement.ComponentCategory, typeof(DataManagement)));
			else if (string.Compare(category, ScriptBundle.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Script bundle", ScriptBundle.ComponentCategory, typeof(ScriptBundle)));
			else if (string.Compare(category, DistributedEvent.ComponentCategory, true) == 0)
				r.Add(new ItemDescriptor("Distributed event", DistributedEvent.ComponentCategory, typeof(DistributedEvent)));
			else if (string.Compare(category, AssemblyFileSystemResource.ComponentCategory, true) == 0)
			{
				r.Add(new ItemDescriptor("Upload assembly", "Upload", typeof(AssemblyUploadResource)));
				r.Add(new ItemDescriptor("File assembly", "File", typeof(AssemblyFileSystemResource)));
			}

			return r;
		}

		public override List<IDomElement> QueryDomRoot(IEnvironment environment)
		{
			return new List<IDomElement>
			{
				new FeaturesElement(environment),
				new ResourcesElement(environment, null),
				new ComponentElement(environment, null, CreateReferences(environment)),
			};
		}

		public override List<IDomElement> QuerySecurityRoot(IDomElement parent)
		{
			return new List<IDomElement>
			{
				new SecurityAreasElement(parent)
			};
		}
	}
}
