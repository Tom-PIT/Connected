using TomPIT.Design.Ide.Properties;

namespace TomPIT.Ide.Designers
{
	public interface IDesignerService
	{
		IPropertyEditor GetPropertyEditor(string name);

		void RegisterPropertyEditor(string name, string view);
	}
}
