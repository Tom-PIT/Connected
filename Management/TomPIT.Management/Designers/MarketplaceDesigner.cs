using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	internal class MarketplaceDesigner : DomDesigner<MarketplaceElement>
	{
		public MarketplaceDesigner(IEnvironment environment, MarketplaceElement element) : base(environment, element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Marketplace.cshtml";
		public override object ViewModel => this;
	}
}
