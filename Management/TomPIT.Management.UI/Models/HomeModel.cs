using TomPIT.Dom;

namespace TomPIT.Models
{
	public class HomeModel : IdeModelBase
	{
		protected override IDom CreateDom()
		{
			var path = string.IsNullOrWhiteSpace(Path)
				? RequestBody?.Optional("path", string.Empty)
				: Path;


			return new Ide.Dom(this, path);
		}

		public override string Id => string.Empty;
		public override string IdeUrl => this.RootUrl();

		protected override void OnDatabinding()
		{
			Title = "Management";
		}
	}
}