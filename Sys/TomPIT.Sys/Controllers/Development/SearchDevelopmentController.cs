using Microsoft.AspNetCore.Mvc;

namespace TomPIT.Sys.Controllers.Development
{
	public class SearchDevelopmentController : SysController
	{
		[HttpPost]
		public void Delete()
		{
			//var body = FromBody();

			//var component = body.Required<Guid>("component");
			//var element = body.Optional("element", Guid.Empty);

			//if (element == Guid.Empty)
			//	DataModel.SysSearch.Enqueue(component);
			//else
			//	DataModel.SysSearch.Enqueue(component, element);
		}
	}
}
