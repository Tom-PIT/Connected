using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Designers
{
	public interface ISupportsAddDesigner : IDomDesigner
	{
		IPropertyProvider Properties { get; }

		string DescriptorId { get; }
	}
}
