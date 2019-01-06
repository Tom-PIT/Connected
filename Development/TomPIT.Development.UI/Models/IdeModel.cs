using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Runtime;

namespace TomPIT.Models
{
	public class IdeModel : IdeModelBase
	{
		private IMicroServiceTemplate _template = null;

		private IMicroServiceTemplate Template
		{
			get
			{
				if (_template == null)
					_template = Connection.GetService<IMicroServiceTemplateService>().Select(MicroService.Template);

				return _template;
			}
		}

		public IMicroService MicroService { get; set; }

		protected override IDom CreateDom()
		{
			var path = string.IsNullOrWhiteSpace(Path)
				? RequestBody?.Optional("path", string.Empty)
				: Path;


			return new Ide.Dom(this, Template, path);
		}


		public override string Id => MicroService.Token.ToString();
		public override string IdeUrl
		{
			get
			{
				return this.RouteUrl("ide", new { microService = MicroService.Url });
			}
		}

		protected override void OnDatabinding()
		{
			Title = MicroService.Name;

			Identity.SetContextId(MicroService.Token.ToString());
		}
	}
}