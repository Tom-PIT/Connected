using TomPIT.Actions;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	internal class DataManagementItemDesigner : CollectionDesigner<DataManagementItemElement>
	{
		public DataManagementItemDesigner(IEnvironment environment, DataManagementItemElement element) : base(environment, element)
		{
		}

		public override bool SupportsReorder => false;

		protected override bool OnCreateToolbarAction(IDesignerToolbarAction action)
		{
			return action.Id != Undo.ActionId;
		}
	}
}
