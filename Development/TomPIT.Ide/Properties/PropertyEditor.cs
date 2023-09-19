using TomPIT.Design.Ide.Properties;

namespace TomPIT.Ide.Properties
{
	public class PropertyEditor : IPropertyEditor
	{
		public string Name { get; set; }

		public string View { get; set; }

		public bool Editable { get; set; }
	}
}
