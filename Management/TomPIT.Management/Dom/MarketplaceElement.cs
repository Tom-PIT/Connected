using TomPIT.ComponentModel;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Dom
{
	public class MarketplaceElement : TomPIT.Dom.Element
	{
		public const string FolderId = "Marketplace";
		private DeploymentDesigner _designer = null;

		public MarketplaceElement(IEnvironment environment) : base(environment, null)
		{
			Id = FolderId;
			Glyph = "fal fa-shopping-cart";
			Title = "Marketplace";
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new DeploymentDesigner(this);

				return _designer;
			}
		}
	}
}
