using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Designers
{
	public interface ITransactionHandler : IDomObject, IEnvironmentObject
	{
		ITransactionResult Execute(string property, string attribute, string value);

		bool Commit(object component, string property, string attribute);

	}
}
