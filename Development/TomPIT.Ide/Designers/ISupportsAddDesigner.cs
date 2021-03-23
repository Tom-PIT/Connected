using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Properties;

namespace TomPIT.Ide.Designers
{
	public interface ISupportsAddDesigner : IDomDesigner
	{
		IPropertyProvider Properties { get; }

		string DescriptorId { get; }
	}
}
