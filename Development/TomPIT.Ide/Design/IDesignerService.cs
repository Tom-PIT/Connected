namespace TomPIT.Design
{
	public interface IDesignerService
	{
		IPropertyEditor GetPropertyEditor(string name);

		void RegisterPropertyEditor(string name, string view);
	}
}
