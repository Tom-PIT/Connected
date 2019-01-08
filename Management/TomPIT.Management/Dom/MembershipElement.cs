using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class MembershipElement : TransactionElement
	{
		public const string FolderId = "Membership";

		private IDomDesigner _designer = null;
		public MembershipElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Title = "Membership";
			Id = FolderId;
		}

		public override bool HasChildren { get { return false; } }

		public override bool Commit(object component, string property, string attribute)
		{
			//SysContext.GetService<IMicroServiceManagementService>().Update(MicroService.Token, MicroService.Name,
			//	MicroService.Status, MicroService.ResourceGroup);

			return true;
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new MembershipDesigner(Environment, this);

				return _designer;
			}
		}
	}
}
