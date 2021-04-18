using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Ide.Dom;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Dom
{
	public class MarketplaceElement : DomElement
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
