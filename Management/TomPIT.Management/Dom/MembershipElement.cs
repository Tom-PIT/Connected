﻿using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Dom
{
	public class MembershipElement : TransactionElement
	{
		public const string FolderId = "Membership";

		private IDomDesigner _designer = null;
		public MembershipElement(IDomElement parent) : base(parent)
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
					_designer = new MembershipDesigner(this);

				return _designer;
			}
		}
	}
}
