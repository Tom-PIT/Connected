using TomPIT.Actions;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	internal class DataManagementItemDesigner : CollectionDesigner<DataManagementItemElement>
	{
		public DataManagementItemDesigner(DataManagementItemElement element) : base(element)
		{
		}

		public override bool SupportsReorder => false;

		protected override bool OnCreateToolbarAction(IDesignerToolbarAction action)
		{
			return action.Id != Undo.ActionId;
		}
	}
}
