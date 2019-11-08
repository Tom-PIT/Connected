using TomPIT.Ide.Environment.Providers;

namespace TomPIT.Ide.Designers
{
	public interface ISupportsAddDesigner : IDomDesigner
	{
		IPropertyProvider Properties { get; }

		string DescriptorId { get; }
	}
}
