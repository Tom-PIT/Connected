using System.Threading.Tasks;
using DevExpress.AspNetCore.Reporting.QueryBuilder;
using DevExpress.AspNetCore.Reporting.QueryBuilder.Native.Services;
using Microsoft.AspNetCore.Mvc;

namespace TomPIT.MicroServices.Reporting.Controllers
{
	public class ReportQueryBuilderController : QueryBuilderController
	{
		public ReportQueryBuilderController(IQueryBuilderMvcControllerService controllerService) : base(controllerService) { }
		public override Task<IActionResult> Invoke()
		{
			return base.Invoke();
		}
	}
}
