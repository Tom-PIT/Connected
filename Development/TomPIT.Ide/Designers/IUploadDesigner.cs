using TomPIT.ComponentModel.Resources;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;

namespace TomPIT.Ide.Designers
{
	public interface IUploadDesigner : IDomDesigner, IEnvironmentObject, IDomObject
	{
		IUploadResource Resource { get; }

		string FileExtension { get; }
	}
}