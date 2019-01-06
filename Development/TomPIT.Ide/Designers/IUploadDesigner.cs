using TomPIT.ComponentModel.Resources;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	public interface IUploadDesigner : IDomDesigner, IEnvironmentClient, IDomClient
	{
		IUploadResource Resource { get; }

		string FileExtension { get; }
	}
}