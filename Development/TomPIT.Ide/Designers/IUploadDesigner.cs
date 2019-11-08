using TomPIT.ComponentModel.Resources;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Designers
{
	public interface IUploadDesigner : IDomDesigner, IEnvironmentObject, IDomObject
	{
		IUploadResource Resource { get; }

		string FileExtension { get; }
	}
}