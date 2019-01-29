using System.Collections.Generic;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Marketplace;

namespace TomPIT.Designers
{
	public class MarketplaceDesigner : DomDesigner<MarketplaceElement>
	{
		private List<IPublishedPackage> _publicPackages = null;

		public MarketplaceDesigner(IEnvironment environment, MarketplaceElement element) : base(environment, element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Marketplace.cshtml";
		public override object ViewModel => this;

		public List<IPublishedPackage> PublicPackages
		{
			get
			{
				if (_publicPackages == null)
					_publicPackages = Connection.GetService<IMarketplaceService>().QueryPublicPackages();

				return _publicPackages;
			}
		}
	}
}
