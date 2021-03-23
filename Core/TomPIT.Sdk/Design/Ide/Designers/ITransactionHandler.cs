using TomPIT.Design.Ide.Dom;

namespace TomPIT.Design.Ide.Designers
{
	public interface ITransactionHandler : IDomObject, IEnvironmentObject
	{
		ITransactionResult Execute(string property, string attribute, string value);

		bool Commit(object component, string property, string attribute);

	}
}
