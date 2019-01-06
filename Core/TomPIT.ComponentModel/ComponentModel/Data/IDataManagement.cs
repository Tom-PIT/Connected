namespace TomPIT.ComponentModel.Data
{
	public interface IDataManagement
	{
		ListItems<IDataManagementItem> Items { get; }
	}
}
