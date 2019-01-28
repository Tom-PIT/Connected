using TomPIT.Designers;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class MarketplaceElement : Element
	{
		public const string FolderId = "Marketplace";
		private MarketplaceDesigner _designer = null;

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
					_designer = new MarketplaceDesigner(Environment, this);

				return _designer;
			}
		}
	}
}
