using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface ITransactionHandler : IDomClient, IEnvironmentClient
	{
		ITransactionResult Execute(string property, string attribute, string value);

		bool Commit(object component, string property, string attribute);

	}
}
