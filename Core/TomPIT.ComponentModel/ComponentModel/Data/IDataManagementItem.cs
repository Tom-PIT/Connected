namespace TomPIT.ComponentModel.Data
{
	public interface IDataManagementItem : IElement
	{
		string Name { get; }

		ListItems<IDataManagementItem> Items { get; }
	}
}
