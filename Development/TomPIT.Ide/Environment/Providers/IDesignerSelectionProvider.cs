namespace TomPIT.Ide.Environment.Providers
{
	public interface IDesignerSelectionProvider
	{
		object Value { get; }
		string SelectionId { get; }
	}
}
