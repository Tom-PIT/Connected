using TomPIT.Application.Resources;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Dom
{
	internal class ResourcesElement : Element
	{
		public const string ElementId = "Resources";

		public ResourcesElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = ElementId;
			Glyph = "fal fa-code";
			Title = "Resources";
		}

		public override bool HasChildren => true;

		public override void LoadChildren()
		{
			LoadConnections();
			LoadLibraries();
			LoadBundles();
			LoadAssemblies();
		}

		private void LoadConnections()
		{
			Items.Add(new CategoryElement(Environment, this, "Connection", "Connections", "Connections", "fal fa-database"));
		}

		private void LoadLibraries()
		{
			Items.Add(new CategoryElement(Environment, this, Library.ComponentCategory, "Libraries", "Libraries", "fal fa-database"));
		}

		private void LoadAssemblies()
		{
			Items.Add(new CategoryElement(Environment, this, AssemblyFileSystemResource.ComponentCategory, "Assemblies", "Assemblies", "fal fa-cubes"));
		}

		private void LoadBundles()
		{
			Items.Add(new CategoryElement(Environment, this, ScriptBundle.ComponentCategory, "Bundles", "Bundles", "fal fa-cubes"));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, "Bundles", true) == 0)
				LoadBundles();
			else if (string.Compare(id, "Assemblies", true) == 0)
				LoadAssemblies();
			else if (string.Compare(id, "Libraries", true) == 0)
				LoadLibraries();
			else if (string.Compare(id, "Connections", true) == 0)
				LoadConnections();
		}
	}
}
