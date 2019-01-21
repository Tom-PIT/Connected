using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.Deployment;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	public class PackageDesigner : DomDesigner<PackageElement>
	{
		public PackageDesigner(IEnvironment environment, PackageElement element) : base(environment, element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Package.cshtml";
		public override object ViewModel => this;

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "create", true) == 0)
				return CreatePackage();

			return base.OnAction(data, action);
		}

		public IDesignerActionResult CreatePackage()
		{
			var r = Package.Create(new PackageCreateArgs(Environment.Context.Connection(), DomQuery.Closest<IMicroServiceScope>(Element).MicroService.Token, new PackageMetaData
			{
				Price = 399
			}, (f) =>
			{

			}));

			return Result.EmptyResult(ViewModel);
		}
	}
}
