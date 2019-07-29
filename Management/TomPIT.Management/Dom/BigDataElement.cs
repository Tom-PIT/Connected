using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Management.Dom
{
	public class BigDataElement : Element
	{
		public const string DomId = "BigData";

		public BigDataElement(IEnvironment environment) : base(environment, null)
		{
			Id = DomId;
			Glyph = "fal fa-database";
			Title = "Big data";

			((Behavior)Behavior).AutoExpand = false;
		}

		public override bool HasChildren { get { return true; } }

		public override void LoadChildren()
		{
			Items.Add(new BigDataNodesElement(Environment, this));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, BigDataNodesElement.FolderId, true) == 0)
				Items.Add(new BigDataNodesElement(Environment, this));
		}
	}
}
